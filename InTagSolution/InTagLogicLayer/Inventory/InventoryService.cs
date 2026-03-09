using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagEntitiesLayer.Inventory;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InTagLogicLayer.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<InventoryService> _logger;
        private readonly IWorkflowHook _workflowHook;


        public InventoryService(IUnitOfWork uow, ILogger<InventoryService> logger, IWorkflowHook workflowHook)
        {
            _uow = uow;
            _logger = logger;
            _workflowHook = workflowHook;
        }

        // ═══════════════════════════════════════
        //  WAREHOUSES & BINS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<WarehouseListVm>> GetWarehousesAsync()
        {
            var warehouses = await _uow.Warehouses.Query()
                .Include(w => w.Bins)
                .OrderBy(w => w.Code).ToListAsync();

            var stockCounts = await _uow.StockItems.Query()
                .GroupBy(s => s.WarehouseId)
                .Select(g => new { WarehouseId = g.Key, Count = g.Count() })
                .ToListAsync();

            return warehouses.Select(w => new WarehouseListVm
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                BinCount = w.Bins.Count,
                StockItemCount = stockCounts.FirstOrDefault(s => s.WarehouseId == w.Id)?.Count ?? 0
            }).ToList();
        }

        public async Task<WarehouseListVm> CreateWarehouseAsync(WarehouseCreateVm model)
        {
            if (await _uow.Warehouses.ExistsAsync(w => w.Code == model.Code))
                throw new InvalidOperationException($"Warehouse code '{model.Code}' already exists.");

            var wh = new Warehouse
            {
                Code = model.Code,
                Name = model.Name,
                Address = model.Address,
                LocationId = model.LocationId
            };
            await _uow.Warehouses.AddAsync(wh);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Warehouse created: {Code}", wh.Code);
            return new WarehouseListVm { Id = wh.Id, Code = wh.Code, Name = wh.Name, Address = wh.Address };
        }

        public async Task<IReadOnlyList<StorageBinListVm>> GetBinsAsync(int warehouseId)
        {
            var bins = await _uow.StorageBins.Query()
                .Where(b => b.WarehouseId == warehouseId && b.IsActive)
                .OrderBy(b => b.BinCode).ToListAsync();
            return bins.Select(b => new StorageBinListVm
            {
                Id = b.Id,
                BinCode = b.BinCode,
                Aisle = b.Aisle,
                Shelf = b.Shelf,
                Level = b.Level
            }).ToList();
        }

        public async Task CreateBinAsync(StorageBinCreateVm model)
        {
            var bin = new StorageBin
            {
                WarehouseId = model.WarehouseId,
                BinCode = model.BinCode,
                Description = model.Description,
                Aisle = model.Aisle,
                Shelf = model.Shelf,
                Level = model.Level
            };
            await _uow.StorageBins.AddAsync(bin);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  STOCK
        // ═══════════════════════════════════════

        public async Task<StockListResultVm> GetStockAsync(StockFilterVm filter)
        {
            var query = _uow.StockItems.Query()
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Include(s => s.StorageBin)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(s => s.Product.ProductCode.Contains(term)
                                         || s.Product.Name.Contains(term)
                                         || (s.LotNumber != null && s.LotNumber.Contains(term)));
            }
            if (filter.WarehouseId.HasValue) query = query.Where(s => s.WarehouseId == filter.WarehouseId.Value);
            if (filter.ProductId.HasValue) query = query.Where(s => s.ProductId == filter.ProductId.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "product" => filter.SortDescending ? query.OrderByDescending(s => s.Product.ProductCode) : query.OrderBy(s => s.Product.ProductCode),
                "warehouse" => filter.SortDescending ? query.OrderByDescending(s => s.Warehouse.Code) : query.OrderBy(s => s.Warehouse.Code),
                "quantity" => filter.SortDescending ? query.OrderByDescending(s => s.QuantityOnHand) : query.OrderBy(s => s.QuantityOnHand),
                "value" => filter.SortDescending ? query.OrderByDescending(s => s.QuantityOnHand * s.UnitCost) : query.OrderBy(s => s.QuantityOnHand * s.UnitCost),
                _ => query.OrderBy(s => s.Product.ProductCode)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            return new StockListResultVm
            {
                Items = items.Select(MapStockItem).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<StockItemDetailVm> GetStockItemAsync(int id)
        {
            var item = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse).Include(s => s.StorageBin)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item == null) throw new KeyNotFoundException("Stock item not found.");
            return MapStockItem(item);
        }

        public async Task UpdateStockLevelsAsync(StockLevelUpdateVm model)
        {
            var item = await _uow.StockItems.GetByIdAsync(model.StockItemId);
            if (item == null) throw new KeyNotFoundException("Stock item not found.");

            item.MinimumLevel = model.MinimumLevel;
            item.MaximumLevel = model.MaximumLevel;
            item.ReorderPoint = model.ReorderPoint;
            item.ReorderQuantity = model.ReorderQuantity;

            _uow.StockItems.Update(item);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  TRANSACTIONS
        // ═══════════════════════════════════════

        public async Task<TransactionListResultVm> GetTransactionsAsync(TransactionFilterVm filter)
        {
            var query = _uow.InventoryTransactions.Query()
                .Include(t => t.Product).Include(t => t.Warehouse)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(t => t.TransactionNumber.Contains(term)
                                         || t.Product.ProductCode.Contains(term)
                                         || (t.ReferenceNumber != null && t.ReferenceNumber.Contains(term)));
            }
            if (filter.Type.HasValue) query = query.Where(t => t.Type == filter.Type.Value);
            if (filter.ProductId.HasValue) query = query.Where(t => t.ProductId == filter.ProductId.Value);
            if (filter.WarehouseId.HasValue) query = query.Where(t => t.WarehouseId == filter.WarehouseId.Value);
            if (filter.DateFrom.HasValue) query = query.Where(t => t.TransactionDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue) query = query.Where(t => t.TransactionDate <= filter.DateTo.Value);

            query = query.OrderByDescending(t => t.TransactionDate);

            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            return new TransactionListResultVm
            {
                Items = items.Select(t => new TransactionListItemVm
                {
                    Id = t.Id,
                    TransactionNumber = t.TransactionNumber,
                    Type = t.Type,
                    ProductCode = t.Product.ProductCode,
                    ProductName = t.Product.Name,
                    WarehouseCode = t.Warehouse.Code,
                    Quantity = t.Quantity,
                    UnitCost = t.UnitCost,
                    TotalCost = t.Quantity * t.UnitCost,
                    TransactionDate = t.TransactionDate,
                    ReferenceNumber = t.ReferenceNumber,
                    LotNumber = t.LotNumber
                }).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<TransactionListItemVm> RecordTransactionAsync(TransactionCreateVm model)
        {
            var product = await _uow.Products.GetByIdAsync(model.ProductId);
            if (product == null) throw new KeyNotFoundException("Product not found.");
            var warehouse = await _uow.Warehouses.GetByIdAsync(model.WarehouseId);
            if (warehouse == null) throw new KeyNotFoundException("Warehouse not found.");

            var txnNumber = await InventoryNumberGenerator.GenerateTransactionAsync(model.Type, _uow);

            var txn = new InventoryTransaction
            {
                TransactionNumber = txnNumber,
                Type = model.Type,
                ProductId = model.ProductId,
                WarehouseId = model.WarehouseId,
                StorageBinId = model.StorageBinId,
                ToWarehouseId = model.ToWarehouseId,
                ToStorageBinId = model.ToStorageBinId,
                Quantity = model.Quantity,
                UnitCost = model.UnitCost,
                TransactionDate = DateTimeOffset.UtcNow,
                ReferenceNumber = model.ReferenceNumber,
                LotNumber = model.LotNumber,
                SerialNumber = model.SerialNumber,
                Reason = model.Reason,
                Notes = model.Notes,
                IsPosted = true
            };

            await _uow.InventoryTransactions.AddAsync(txn);

            // Update stock
            await ApplyTransactionToStock(model);

            await _uow.SaveChangesAsync();

            _logger.LogInformation("Inventory transaction {Number}: {Type} {Qty} of {Product} @ {WH}",
                txnNumber, model.Type, model.Quantity, product.ProductCode, warehouse.Code);

            return new TransactionListItemVm
            {
                Id = txn.Id,
                TransactionNumber = txn.TransactionNumber,
                Type = txn.Type,
                ProductCode = product.ProductCode,
                ProductName = product.Name,
                WarehouseCode = warehouse.Code,
                Quantity = txn.Quantity,
                UnitCost = txn.UnitCost,
                TotalCost = txn.Quantity * txn.UnitCost,
                TransactionDate = txn.TransactionDate,
                ReferenceNumber = txn.ReferenceNumber,
                LotNumber = txn.LotNumber
            };
        }

        private async Task ApplyTransactionToStock(TransactionCreateVm model)
        {
            var stock = await _uow.StockItems.GetStockAsync(
                model.ProductId, model.WarehouseId, model.StorageBinId, model.LotNumber);

            switch (model.Type)
            {
                case TransactionType.Receipt:
                case TransactionType.Return:
                case TransactionType.ProductionOutput:
                    if (stock == null)
                    {
                        stock = new StockItem
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.WarehouseId,
                            StorageBinId = model.StorageBinId,
                            LotNumber = model.LotNumber,
                            SerialNumber = model.SerialNumber,
                            ValuationMethod = ValuationMethod.WeightedAverage
                        };
                        await _uow.StockItems.AddAsync(stock);
                    }
                    // Weighted average cost
                    var totalExistingValue = stock.QuantityOnHand * stock.UnitCost;
                    var newValue = model.Quantity * model.UnitCost;
                    var newTotalQty = stock.QuantityOnHand + model.Quantity;
                    stock.UnitCost = newTotalQty > 0 ? Math.Round((totalExistingValue + newValue) / newTotalQty, 4) : model.UnitCost;
                    stock.QuantityOnHand = newTotalQty;
                    break;

                case TransactionType.Issue:
                case TransactionType.Scrap:
                case TransactionType.ProductionConsumption:
                    if (stock == null) throw new InvalidOperationException("No stock found for this product/warehouse combination.");
                    if (stock.QuantityOnHand < model.Quantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {stock.QuantityOnHand}, Requested: {model.Quantity}.");
                    stock.QuantityOnHand -= model.Quantity;
                    break;

                case TransactionType.Transfer:
                    if (stock == null) throw new InvalidOperationException("No stock found at source.");
                    if (stock.QuantityOnHand < model.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for transfer. Available: {stock.QuantityOnHand}.");
                    if (!model.ToWarehouseId.HasValue)
                        throw new InvalidOperationException("Destination warehouse required for transfers.");

                    stock.QuantityOnHand -= model.Quantity;

                    var destStock = await _uow.StockItems.GetStockAsync(
                        model.ProductId, model.ToWarehouseId.Value, model.ToStorageBinId, model.LotNumber);
                    if (destStock == null)
                    {
                        destStock = new StockItem
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.ToWarehouseId.Value,
                            StorageBinId = model.ToStorageBinId,
                            LotNumber = model.LotNumber,
                            UnitCost = stock.UnitCost,
                            ValuationMethod = stock.ValuationMethod
                        };
                        await _uow.StockItems.AddAsync(destStock);
                    }
                    destStock.QuantityOnHand += model.Quantity;
                    _uow.StockItems.Update(destStock);
                    break;

                case TransactionType.Adjustment:
                    if (stock == null)
                    {
                        stock = new StockItem
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.WarehouseId,
                            StorageBinId = model.StorageBinId,
                            LotNumber = model.LotNumber,
                            UnitCost = model.UnitCost,
                            ValuationMethod = ValuationMethod.WeightedAverage
                        };
                        await _uow.StockItems.AddAsync(stock);
                    }
                    stock.QuantityOnHand += model.Quantity; // can be negative
                    break;
            }

            if (stock != null) _uow.StockItems.Update(stock);
            // Check reorder trigger
            if (stock != null && stock.ReorderPoint > 0 && stock.QuantityOnHand <= stock.ReorderPoint && stock.QuantityOnHand > 0)
            {
                var product = await _uow.Products.GetByIdAsync(stock.ProductId);
                var warehouse = await _uow.Warehouses.GetByIdAsync(stock.WarehouseId);
                if (product != null && warehouse != null)
                    await _workflowHook.OnStockBelowReorderAsync(stock.Id, product.ProductCode, warehouse.Code, stock.QuantityOnHand, stock.ReorderPoint);
            }
        }

        // ═══════════════════════════════════════
        //  CYCLE COUNTS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<CycleCountListItemVm>> GetCycleCountsAsync()
        {
            var counts = await _uow.CycleCounts.Query()
                .Include(c => c.Warehouse)
                .Include(c => c.Lines)
                .OrderByDescending(c => c.CountDate).ToListAsync();

            return counts.Select(c => new CycleCountListItemVm
            {
                Id = c.Id,
                CountNumber = c.CountNumber,
                WarehouseName = c.Warehouse.Name,
                CountDate = c.CountDate,
                IsCompleted = c.IsCompleted,
                LineCount = c.Lines.Count
            }).ToList();
        }

        public async Task<CycleCountDetailVm> GetCycleCountByIdAsync(int id)
        {
            var cc = await _uow.CycleCounts.Query()
                .Include(c => c.Warehouse)
                .Include(c => c.Lines).ThenInclude(l => l.Product)
                .Include(c => c.Lines).ThenInclude(l => l.StorageBin)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cc == null) throw new KeyNotFoundException("Cycle count not found.");
            return MapCycleCount(cc);
        }

        public async Task<CycleCountDetailVm> CreateCycleCountAsync(CycleCountCreateVm model)
        {
            var wh = await _uow.Warehouses.GetByIdAsync(model.WarehouseId);
            if (wh == null) throw new KeyNotFoundException("Warehouse not found.");

            var number = await InventoryNumberGenerator.GenerateCycleCountAsync(_uow);

            var cc = new CycleCount
            {
                CountNumber = number,
                WarehouseId = model.WarehouseId,
                CountDate = DateTimeOffset.UtcNow,
                Notes = model.Notes
            };
            await _uow.CycleCounts.AddAsync(cc);
            await _uow.SaveChangesAsync();

            // Populate lines from current stock
            var stockItems = await _uow.StockItems.GetByWarehouseAsync(model.WarehouseId);
            foreach (var s in stockItems)
            {
                var line = new CycleCountLine
                {
                    CycleCountId = cc.Id,
                    ProductId = s.ProductId,
                    StorageBinId = s.StorageBinId,
                    SystemQuantity = s.QuantityOnHand,
                    CountedQuantity = 0
                };
                await _uow.CycleCountLines.AddAsync(line);
            }
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Cycle count {Number} created for warehouse {WH} with {Lines} lines",
                number, wh.Code, stockItems.Count);
            return await GetCycleCountByIdAsync(cc.Id);
        }

        public async Task UpdateCycleCountLineAsync(CycleCountLineUpdateVm model)
        {
            var line = await _uow.CycleCountLines.GetByIdAsync(model.CycleCountLineId);
            if (line == null) throw new KeyNotFoundException("Cycle count line not found.");
            line.CountedQuantity = model.CountedQuantity;
            _uow.CycleCountLines.Update(line);
            await _uow.SaveChangesAsync();
        }

        public async Task<CycleCountDetailVm> CompleteCycleCountAsync(int id)
        {
            var cc = await _uow.CycleCounts.Query()
                .Include(c => c.Warehouse)
                .Include(c => c.Lines).ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cc == null) throw new KeyNotFoundException("Cycle count not found.");
            if (cc.IsCompleted) throw new InvalidOperationException("Cycle count already completed.");

            foreach (var line in cc.Lines.Where(l => l.Variance != 0))
            {
                var adjQty = line.Variance;
                var stock = await _uow.StockItems.GetStockAsync(
                    line.ProductId, cc.WarehouseId, line.StorageBinId);

                if (stock != null)
                {
                    // Record adjustment transaction
                    var txnNum = await InventoryNumberGenerator.GenerateTransactionAsync(TransactionType.CycleCount, _uow);
                    var txn = new InventoryTransaction
                    {
                        TransactionNumber = txnNum,
                        Type = TransactionType.CycleCount,
                        ProductId = line.ProductId,
                        WarehouseId = cc.WarehouseId,
                        StorageBinId = line.StorageBinId,
                        Quantity = adjQty,
                        UnitCost = stock.UnitCost,
                        TransactionDate = DateTimeOffset.UtcNow,
                        ReferenceNumber = cc.CountNumber,
                        Reason = $"Cycle count variance: system {line.SystemQuantity}, counted {line.CountedQuantity}"
                    };
                    await _uow.InventoryTransactions.AddAsync(txn);

                    stock.QuantityOnHand = line.CountedQuantity;
                    _uow.StockItems.Update(stock);
                }

                line.IsAdjusted = true;
                _uow.CycleCountLines.Update(line);
            }

            cc.IsCompleted = true;
            cc.CompletedDate = DateTimeOffset.UtcNow;
            _uow.CycleCounts.Update(cc);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Cycle count {Number} completed with {Adj} adjustments",
                cc.CountNumber, cc.Lines.Count(l => l.IsAdjusted));

            await _workflowHook.OnCycleCountCompletedAsync(
                cc.CountNumber,
                cc.Lines.Count(l => l.Variance != 0),
                cc.Lines.Sum(l => Math.Abs(l.CountedQuantity - l.SystemQuantity)));
            return await GetCycleCountByIdAsync(id);
        }

        // ═══════════════════════════════════════
        //  VALUATION & REORDER
        // ═══════════════════════════════════════

        public async Task<InventoryValuationVm> GetValuationAsync()
        {
            var allStock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .Where(s => s.QuantityOnHand > 0).ToListAsync();

            var byWarehouse = allStock.GroupBy(s => s.WarehouseId)
                .Select(g =>
                {
                    var first = g.First();
                    return new ValuationByWarehouseVm
                    {
                        WarehouseCode = first.Warehouse.Code,
                        WarehouseName = first.Warehouse.Name,
                        SKUs = g.Count(),
                        TotalQuantity = g.Sum(s => s.QuantityOnHand),
                        TotalValue = g.Sum(s => s.QuantityOnHand * s.UnitCost)
                    };
                }).OrderByDescending(w => w.TotalValue).ToList();

            var byProduct = allStock.GroupBy(s => s.ProductId)
                .Select(g =>
                {
                    var first = g.First();
                    var totalQty = g.Sum(s => s.QuantityOnHand);
                    var totalVal = g.Sum(s => s.QuantityOnHand * s.UnitCost);
                    return new ValuationByProductVm
                    {
                        ProductCode = first.Product.ProductCode,
                        ProductName = first.Product.Name,
                        TotalQuantity = totalQty,
                        UnitCost = totalQty > 0 ? Math.Round(totalVal / totalQty, 4) : 0,
                        TotalValue = totalVal,
                        Method = first.ValuationMethod
                    };
                }).OrderByDescending(p => p.TotalValue).ToList();

            return new InventoryValuationVm
            {
                TotalValue = allStock.Sum(s => s.QuantityOnHand * s.UnitCost),
                TotalSKUs = allStock.Count,
                ByWarehouse = byWarehouse,
                ByProduct = byProduct
            };
        }

        public async Task<ReorderReportVm> GetReorderReportAsync()
        {
            var allStock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .Where(s => s.ReorderPoint > 0 || s.MinimumLevel > 0)
                .ToListAsync();

            var items = allStock.Select(s =>
            {
                var status = s.QuantityOnHand <= 0 ? ReorderStatus.OutOfStock
                    : s.QuantityOnHand <= s.MinimumLevel && s.MinimumLevel > 0 ? ReorderStatus.BelowMinimum
                    : s.QuantityOnHand <= s.ReorderPoint && s.ReorderPoint > 0 ? ReorderStatus.BelowReorderPoint
                    : s.MaximumLevel > 0 && s.QuantityOnHand > s.MaximumLevel ? ReorderStatus.Overstock
                    : ReorderStatus.Normal;

                var suggestedQty = status == ReorderStatus.Normal ? 0
                    : s.ReorderQuantity > 0 ? s.ReorderQuantity
                    : s.MaximumLevel > 0 ? s.MaximumLevel - s.QuantityOnHand : 0;

                return new ReorderItemVm
                {
                    StockItemId = s.Id,
                    ProductCode = s.Product.ProductCode,
                    ProductName = s.Product.Name,
                    WarehouseCode = s.Warehouse.Code,
                    QuantityOnHand = s.QuantityOnHand,
                    ReorderPoint = s.ReorderPoint,
                    MinimumLevel = s.MinimumLevel,
                    ReorderQuantity = s.ReorderQuantity,
                    SuggestedOrderQty = Math.Max(0, suggestedQty),
                    Status = status
                };
            }).Where(i => i.Status != ReorderStatus.Normal)
              .OrderBy(i => i.Status).ThenBy(i => i.QuantityOnHand).ToList();

            return new ReorderReportVm
            {
                BelowReorderCount = items.Count(i => i.Status == ReorderStatus.BelowReorderPoint),
                OutOfStockCount = items.Count(i => i.Status == ReorderStatus.OutOfStock),
                BelowMinimumCount = items.Count(i => i.Status == ReorderStatus.BelowMinimum),
                Items = items
            };
        }

        // ═══════════════════════════════════════
        //  MAPPING
        // ═══════════════════════════════════════

        private static StockItemDetailVm MapStockItem(StockItem s)
        {
            var reorderStatus = s.QuantityOnHand <= 0 ? ReorderStatus.OutOfStock
                : s.MinimumLevel > 0 && s.QuantityOnHand <= s.MinimumLevel ? ReorderStatus.BelowMinimum
                : s.ReorderPoint > 0 && s.QuantityOnHand <= s.ReorderPoint ? ReorderStatus.BelowReorderPoint
                : s.MaximumLevel > 0 && s.QuantityOnHand > s.MaximumLevel ? ReorderStatus.Overstock
                : ReorderStatus.Normal;

            return new StockItemDetailVm
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductCode = s.Product.ProductCode,
                ProductName = s.Product.Name,
                UOM = s.Product.UOM.ToString(),
                WarehouseId = s.WarehouseId,
                WarehouseCode = s.Warehouse.Code,
                WarehouseName = s.Warehouse.Name,
                BinCode = s.StorageBin?.BinCode,
                QuantityOnHand = s.QuantityOnHand,
                QuantityReserved = s.QuantityReserved,
                QuantityAvailable = s.QuantityOnHand - s.QuantityReserved,
                Status = s.Status,
                MinimumLevel = s.MinimumLevel,
                MaximumLevel = s.MaximumLevel,
                ReorderPoint = s.ReorderPoint,
                ReorderQuantity = s.ReorderQuantity,
                ValuationMethod = s.ValuationMethod,
                UnitCost = s.UnitCost,
                TotalValue = s.QuantityOnHand * s.UnitCost,
                LotNumber = s.LotNumber,
                SerialNumber = s.SerialNumber,
                ExpiryDate = s.ExpiryDate,
                ReorderStatus = reorderStatus
            };
        }

        private static CycleCountDetailVm MapCycleCount(CycleCount cc)
        {
            var lines = cc.Lines.Select(l => new CycleCountLineVm
            {
                Id = l.Id,
                ProductCode = l.Product.ProductCode,
                ProductName = l.Product.Name,
                BinCode = l.StorageBin?.BinCode,
                SystemQuantity = l.SystemQuantity,
                CountedQuantity = l.CountedQuantity,
                Variance = l.CountedQuantity - l.SystemQuantity,
                VariancePercent = l.SystemQuantity != 0
                    ? Math.Round((l.CountedQuantity - l.SystemQuantity) / l.SystemQuantity * 100, 2) : 0,
                IsAdjusted = l.IsAdjusted
            }).ToList();

            return new CycleCountDetailVm
            {
                Id = cc.Id,
                CountNumber = cc.CountNumber,
                WarehouseName = cc.Warehouse.Name,
                CountDate = cc.CountDate,
                IsCompleted = cc.IsCompleted,
                CompletedDate = cc.CompletedDate,
                Notes = cc.Notes,
                Lines = lines,
                TotalLines = lines.Count,
                VarianceCount = lines.Count(l => l.Variance != 0),
                TotalVarianceValue = lines.Sum(l => Math.Abs(l.Variance))
            };
        }

        // ═══════════════════════════════════════
        //  ABC ANALYSIS
        // ═══════════════════════════════════════

        public async Task<ABCAnalysisVm> GetABCAnalysisAsync()
        {
            var allStock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .Where(s => s.QuantityOnHand > 0)
                .ToListAsync();

            var totalValue = allStock.Sum(s => s.QuantityOnHand * s.UnitCost);
            if (totalValue == 0) return new ABCAnalysisVm { TotalSKUs = allStock.Count };

            // Sort by value descending, assign cumulative %
            var ranked = allStock
                .Select(s => new { Stock = s, Value = s.QuantityOnHand * s.UnitCost })
                .OrderByDescending(x => x.Value)
                .ToList();

            var items = new List<ABCItemVm>();
            var cumulative = 0m;
            var rank = 0;

            foreach (var r in ranked)
            {
                rank++;
                cumulative += r.Value;
                var cumulativePct = Math.Round(cumulative / totalValue * 100, 2);

                var category = cumulativePct <= 80 ? "A" : cumulativePct <= 95 ? "B" : "C";

                items.Add(new ABCItemVm
                {
                    StockItemId = r.Stock.Id,
                    ProductCode = r.Stock.Product.ProductCode,
                    ProductName = r.Stock.Product.Name,
                    WarehouseCode = r.Stock.Warehouse.Code,
                    QuantityOnHand = r.Stock.QuantityOnHand,
                    UnitCost = r.Stock.UnitCost,
                    TotalValue = r.Value,
                    CumulativePercent = cumulativePct,
                    Category = category,
                    Rank = rank
                });
            }

            var categories = items.GroupBy(i => i.Category)
                .Select(g => new ABCCategoryVm
                {
                    Category = g.Key,
                    SKUs = g.Count(),
                    SKUPercent = allStock.Count > 0 ? Math.Round(g.Count() * 100m / allStock.Count, 1) : 0,
                    TotalValue = g.Sum(i => i.TotalValue),
                    ValuePercent = Math.Round(g.Sum(i => i.TotalValue) / totalValue * 100, 1)
                }).OrderBy(c => c.Category).ToList();

            return new ABCAnalysisVm
            {
                TotalInventoryValue = totalValue,
                TotalSKUs = allStock.Count,
                Categories = categories,
                Items = items
            };
        }

        // ═══════════════════════════════════════
        //  STOCK AGING
        // ═══════════════════════════════════════

        public async Task<StockAgingVm> GetStockAgingAsync()
        {
            var now = DateTimeOffset.UtcNow;

            var allStock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .Where(s => s.QuantityOnHand > 0)
                .ToListAsync();

            // Get last movement date per product+warehouse from transactions
            var lastMovements = await _uow.InventoryTransactions.Query()
                .GroupBy(t => new { t.ProductId, t.WarehouseId })
                .Select(g => new { g.Key.ProductId, g.Key.WarehouseId, LastDate = g.Max(t => t.TransactionDate) })
                .ToListAsync();

            var agingItems = allStock.Select(s =>
            {
                var lastMove = lastMovements
                    .FirstOrDefault(m => m.ProductId == s.ProductId && m.WarehouseId == s.WarehouseId);
                var lastDate = lastMove?.LastDate ?? s.CreatedDate;
                var daysSince = (int)(now - lastDate).TotalDays;

                var bucket = daysSince <= 30 ? "0-30 days"
                    : daysSince <= 60 ? "31-60 days"
                    : daysSince <= 90 ? "61-90 days"
                    : daysSince <= 180 ? "91-180 days"
                    : daysSince <= 365 ? "181-365 days"
                    : "365+ days";

                return new AgingItemVm
                {
                    StockItemId = s.Id,
                    ProductCode = s.Product.ProductCode,
                    ProductName = s.Product.Name,
                    WarehouseCode = s.Warehouse.Code,
                    QuantityOnHand = s.QuantityOnHand,
                    TotalValue = s.QuantityOnHand * s.UnitCost,
                    DaysSinceLastMovement = daysSince,
                    LastMovementDate = lastDate,
                    AgingBucket = bucket
                };
            }).OrderByDescending(i => i.DaysSinceLastMovement).ToList();

            var totalValue = agingItems.Sum(i => i.TotalValue);
            var slowMovingValue = agingItems.Where(i => i.DaysSinceLastMovement > 90).Sum(i => i.TotalValue);

            var bucketOrder = new[] { "0-30 days", "31-60 days", "61-90 days", "91-180 days", "181-365 days", "365+ days" };
            var buckets = bucketOrder.Select(label =>
            {
                var items = agingItems.Where(i => i.AgingBucket == label).ToList();
                return new AgingBucketVm
                {
                    Label = label,
                    SKUs = items.Count,
                    TotalValue = items.Sum(i => i.TotalValue),
                    Percentage = totalValue > 0 ? Math.Round(items.Sum(i => i.TotalValue) / totalValue * 100, 1) : 0
                };
            }).ToList();

            return new StockAgingVm
            {
                Buckets = buckets,
                Items = agingItems,
                TotalValue = totalValue,
                SlowMovingValue = slowMovingValue,
                SlowMovingPercent = totalValue > 0 ? Math.Round(slowMovingValue / totalValue * 100, 1) : 0
            };
        }

        // ═══════════════════════════════════════
        //  EXPIRY TRACKING
        // ═══════════════════════════════════════

        public async Task<ExpiryReportVm> GetExpiryReportAsync(int daysThreshold = 30)
        {
            var now = DateTimeOffset.UtcNow;
            var threshold = now.AddDays(daysThreshold);

            var itemsWithExpiry = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .Where(s => s.ExpiryDate.HasValue && s.QuantityOnHand > 0)
                .OrderBy(s => s.ExpiryDate)
                .ToListAsync();

            var expiryItems = itemsWithExpiry.Select(s =>
            {
                var daysUntil = (int)(s.ExpiryDate!.Value - now).TotalDays;
                var status = daysUntil < 0 ? "Expired" : daysUntil <= daysThreshold ? "Expiring Soon" : "OK";

                return new ExpiryItemVm
                {
                    StockItemId = s.Id,
                    ProductCode = s.Product.ProductCode,
                    ProductName = s.Product.Name,
                    WarehouseCode = s.Warehouse.Code,
                    LotNumber = s.LotNumber,
                    QuantityOnHand = s.QuantityOnHand,
                    TotalValue = s.QuantityOnHand * s.UnitCost,
                    ExpiryDate = s.ExpiryDate!.Value,
                    DaysUntilExpiry = daysUntil,
                    Status = status
                };
            }).Where(i => i.Status != "OK").ToList();

            return new ExpiryReportVm
            {
                ExpiredCount = expiryItems.Count(i => i.Status == "Expired"),
                ExpiringSoonCount = expiryItems.Count(i => i.Status == "Expiring Soon"),
                ExpiredValue = expiryItems.Where(i => i.Status == "Expired").Sum(i => i.TotalValue),
                Items = expiryItems
            };
        }

        // ═══════════════════════════════════════
        //  INVENTORY TURNOVER
        // ═══════════════════════════════════════

        public async Task<InventoryTurnoverVm> GetTurnoverAnalysisAsync(int months = 12)
        {
            var now = DateTimeOffset.UtcNow;
            var periodStart = now.AddMonths(-months);

            // Current stock value
            var currentStock = await _uow.StockItems.Query()
                .Include(s => s.Product)
                .Where(s => s.QuantityOnHand > 0)
                .ToListAsync();
            var currentValue = currentStock.Sum(s => s.QuantityOnHand * s.UnitCost);

            // Issues (COGS proxy) in the period
            var issueTypes = new[] { TransactionType.Issue, TransactionType.ProductionConsumption, TransactionType.Scrap };
            var issues = await _uow.InventoryTransactions.Query()
                .Include(t => t.Product)
                .Where(t => issueTypes.Contains(t.Type) && t.TransactionDate >= periodStart)
                .ToListAsync();

            var cogsValue = issues.Sum(t => t.Quantity * t.UnitCost);

            // Estimate average inventory = (current + beginning) / 2
            // Simplified: use current value as proxy (beginning data may not be available)
            var avgInventory = currentValue > 0 ? currentValue : 1;
            var turnoverRatio = avgInventory > 0 ? Math.Round(cogsValue / avgInventory, 2) : 0;
            var daysOnHand = turnoverRatio > 0 ? Math.Round(365m / turnoverRatio, 0) : 999;

            // By product
            var byProduct = currentStock.GroupBy(s => s.ProductId).Select(g =>
            {
                var first = g.First();
                var avgStock = g.Sum(s => s.QuantityOnHand);
                var totalIssued = issues
                    .Where(t => t.ProductId == first.ProductId)
                    .Sum(t => t.Quantity);

                var ratio = avgStock > 0 ? Math.Round(totalIssued / avgStock, 2) : 0;
                var doh = ratio > 0 ? Math.Round(365m / ratio, 0) : 999;

                var performance = ratio >= 6 ? "Fast" : ratio >= 2 ? "Normal" : ratio >= 0.5m ? "Slow" : "Dead";

                return new TurnoverByProductVm
                {
                    ProductCode = first.Product.ProductCode,
                    ProductName = first.Product.Name,
                    AverageStock = avgStock,
                    TotalIssued = totalIssued,
                    TurnoverRatio = ratio,
                    DaysOnHand = doh,
                    Performance = performance
                };
            }).OrderBy(p => p.TurnoverRatio).ToList();

            return new InventoryTurnoverVm
            {
                AverageInventoryValue = avgInventory,
                COGSValue = cogsValue,
                TurnoverRatio = turnoverRatio,
                DaysOnHand = daysOnHand,
                ByProduct = byProduct
            };
        }
        public async Task<InventoryDashboardVm> GetDashboardAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var allStock = await _uow.StockItems.Query()
                .Include(s => s.Product).Include(s => s.Warehouse)
                .ToListAsync();

            var allTxns = await _uow.InventoryTransactions.Query()
                .Include(t => t.Product).Include(t => t.Warehouse)
                .ToListAsync();

            var warehouses = await _uow.Warehouses.GetAllAsync();
            var totalValue = allStock.Where(s => s.QuantityOnHand > 0).Sum(s => s.QuantityOnHand * s.UnitCost);

            // Reorder alerts
            var stockAlerts = allStock
                .Where(s => s.QuantityOnHand <= 0
                            || (s.MinimumLevel > 0 && s.QuantityOnHand <= s.MinimumLevel)
                            || (s.ReorderPoint > 0 && s.QuantityOnHand <= s.ReorderPoint))
                .Select(s => new StockAlertVm
                {
                    StockItemId = s.Id,
                    ProductCode = s.Product.ProductCode,
                    ProductName = s.Product.Name,
                    WarehouseCode = s.Warehouse.Code,
                    QuantityOnHand = s.QuantityOnHand,
                    ReorderPoint = s.ReorderPoint,
                    Status = s.QuantityOnHand <= 0 ? ReorderStatus.OutOfStock
                        : s.MinimumLevel > 0 && s.QuantityOnHand <= s.MinimumLevel ? ReorderStatus.BelowMinimum
                        : ReorderStatus.BelowReorderPoint
                }).OrderBy(a => a.Status).ThenBy(a => a.QuantityOnHand).Take(15).ToList();

            // Expiry alerts
            var expiryAlerts = allStock
                .Where(s => s.ExpiryDate.HasValue && s.QuantityOnHand > 0
                            && s.ExpiryDate <= now.AddDays(30))
                .Select(s => new ExpiryAlertVm
                {
                    ProductCode = s.Product.ProductCode,
                    ProductName = s.Product.Name,
                    LotNumber = s.LotNumber,
                    Quantity = s.QuantityOnHand,
                    ExpiryDate = s.ExpiryDate!.Value,
                    DaysRemaining = (int)(s.ExpiryDate!.Value - now).TotalDays,
                    IsExpired = s.ExpiryDate < now
                }).OrderBy(e => e.ExpiryDate).Take(10).ToList();

            // Slow-moving (no transaction in 90+ days)
            var lastMovements = allTxns
                .GroupBy(t => new { t.ProductId, t.WarehouseId })
                .ToDictionary(g => g.Key, g => g.Max(t => t.TransactionDate));

            var slowMoving = allStock.Where(s => s.QuantityOnHand > 0).Where(s =>
            {
                var key = new { s.ProductId, s.WarehouseId };
                var lastDate = lastMovements.ContainsKey(key) ? lastMovements[key] : s.CreatedDate;
                return (now - lastDate).TotalDays > 90;
            }).ToList();

            // By warehouse
            var byWarehouse = allStock.Where(s => s.QuantityOnHand > 0)
                .GroupBy(s => s.WarehouseId)
                .Select(g =>
                {
                    var first = g.First();
                    var val = g.Sum(s => s.QuantityOnHand * s.UnitCost);
                    return new ValueByWarehouseVm
                    {
                        Code = first.Warehouse.Code,
                        Name = first.Warehouse.Name,
                        SKUs = g.Count(),
                        Value = val,
                        Percentage = totalValue > 0 ? Math.Round(val / totalValue * 100, 1) : 0
                    };
                }).OrderByDescending(w => w.Value).ToList();

            // Top value items
            var topValue = allStock.Where(s => s.QuantityOnHand > 0)
                .OrderByDescending(s => s.QuantityOnHand * s.UnitCost).Take(5)
                .Select(s => new TopValueItemVm
                {
                    ProductCode = s.Product.ProductCode,
                    ProductName = s.Product.Name,
                    Quantity = s.QuantityOnHand,
                    TotalValue = s.QuantityOnHand * s.UnitCost
                }).ToList();

            // Transaction summary (this month by type)
            var monthTxns = allTxns.Where(t => t.TransactionDate >= monthStart).ToList();
            var txnSummary = monthTxns.GroupBy(t => t.Type)
                .Select(g => new TransactionSummaryVm
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    TotalQuantity = g.Sum(t => t.Quantity),
                    TotalValue = g.Sum(t => t.Quantity * t.UnitCost)
                }).OrderByDescending(t => t.Count).ToList();

            // Turnover
            var issueTypes = new[] { TransactionType.Issue, TransactionType.ProductionConsumption, TransactionType.Scrap };
            var cogsYear = allTxns.Where(t => issueTypes.Contains(t.Type) && t.TransactionDate >= now.AddMonths(-12))
                .Sum(t => t.Quantity * t.UnitCost);
            var turnover = totalValue > 0 ? Math.Round(cogsYear / totalValue, 2) : 0;

            // Recent transactions
            var recentTxns = allTxns.OrderByDescending(t => t.TransactionDate).Take(10)
                .Select(t =>
                {
                    var inbound = t.Type == TransactionType.Receipt || t.Type == TransactionType.Return || t.Type == TransactionType.ProductionOutput;
                    return new RecentTxnVm
                    {
                        Icon = inbound ? "bi-box-arrow-in-down" : t.Type == TransactionType.Transfer ? "bi-arrow-left-right" : "bi-box-arrow-up",
                        Description = $"{t.TransactionNumber}: {t.Type} — {t.Product.ProductCode} × {t.Quantity:N0} @ {t.Warehouse.Code}",
                        Date = t.TransactionDate
                    };
                }).ToList();

            return new InventoryDashboardVm
            {
                TotalInventoryValue = totalValue,
                TotalSKUs = allStock.Count(s => s.QuantityOnHand > 0),
                WarehouseCount = warehouses.Count,
                TurnoverRatio = turnover,
                TransactionsThisMonth = monthTxns.Count,
                OutOfStockCount = allStock.Count(s => s.QuantityOnHand <= 0),
                BelowReorderCount = allStock.Count(s => s.ReorderPoint > 0 && s.QuantityOnHand <= s.ReorderPoint && s.QuantityOnHand > 0),
                ExpiredCount = expiryAlerts.Count(e => e.IsExpired),
                ExpiringSoonCount = expiryAlerts.Count(e => !e.IsExpired),
                SlowMovingSKUs = slowMoving.Count,
                SlowMovingValue = slowMoving.Sum(s => s.QuantityOnHand * s.UnitCost),
                StockAlerts = stockAlerts,
                ExpiryAlerts = expiryAlerts,
                ByWarehouse = byWarehouse,
                TopValueItems = topValue,
                TransactionSummary = txnSummary,
                RecentTransactions = recentTxns
            };
        }

        public async Task<byte[]> ExportStockReportAsync()
        {
            var export = new InventoryExportService(_uow);
            return await export.ExportStockReportAsync();
        }

        public async Task<byte[]> ExportTransactionReportAsync(TransactionFilterVm filter)
        {
            var export = new InventoryExportService(_uow);
            return await export.ExportTransactionReportAsync(filter);
        }

        public async Task<byte[]> ExportValuationReportAsync()
        {
            var export = new InventoryExportService(_uow);
            return await export.ExportValuationReportAsync();
        }
    }
}
