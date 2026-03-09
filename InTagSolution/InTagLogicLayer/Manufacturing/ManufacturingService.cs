using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Manufacturing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InTagLogicLayer.Manufacturing
{
    public class ManufacturingService : IManufacturingService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ManufacturingService> _logger;
        private readonly IWorkflowHook _workflowHook;


        public ManufacturingService(IUnitOfWork uow, ILogger<ManufacturingService> logger, IWorkflowHook workflowHook)
        {
            _uow = uow;
            _logger = logger;
            _workflowHook = workflowHook;
        }

        // ═══════════════════════════════════════
        //  PRODUCTS
        // ═══════════════════════════════════════

        public async Task<ProductDetailVm> GetProductByIdAsync(int id)
        {
            var p = await _uow.Products.Query()
                .Include(x => x.BOMs)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) throw new KeyNotFoundException("Product not found.");
            return new ProductDetailVm
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                Description = p.Description,
                UOM = p.UOM.ToString(),
                Category = p.Category,
                IsRawMaterial = p.IsRawMaterial,
                IsFinishedGood = p.IsFinishedGood,
                StandardCost = p.StandardCost,
                Barcode = p.Barcode,
                Notes = p.Notes,
                BOMCount = p.BOMs?.Count ?? 0
            };
        }

        public async Task<IReadOnlyList<ProductListItemVm>> GetProductsAsync()
        {
            var products = await _uow.Products.Query()
                .OrderBy(p => p.ProductCode).ToListAsync();
            return products.Select(p => new ProductListItemVm
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                UOM = p.UOM.ToString(),
                Category = p.Category,
                IsRawMaterial = p.IsRawMaterial,
                IsFinishedGood = p.IsFinishedGood,
                StandardCost = p.StandardCost
            }).ToList();
        }

        public async Task<ProductDetailVm> CreateProductAsync(ProductCreateVm model)
        {
            if (await _uow.Products.ExistsAsync(p => p.ProductCode == model.ProductCode))
                throw new InvalidOperationException($"Product code '{model.ProductCode}' already exists.");

            var product = new Product
            {
                ProductCode = model.ProductCode,
                Name = model.Name,
                Description = model.Description,
                UOM = model.UOM,
                Category = model.Category,
                IsRawMaterial = model.IsRawMaterial,
                IsFinishedGood = model.IsFinishedGood,
                StandardCost = model.StandardCost,
                Barcode = model.Barcode,
                Notes = model.Notes
            };

            await _uow.Products.AddAsync(product);
            await _uow.SaveChangesAsync();
            _logger.LogInformation("Product created: {Code}", product.ProductCode);
            return await GetProductByIdAsync(product.Id);
        }

        public async Task<ProductDetailVm> UpdateProductAsync(int id, ProductCreateVm model)
        {
            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            product.Name = model.Name;
            product.Description = model.Description;
            product.UOM = model.UOM;
            product.Category = model.Category;
            product.IsRawMaterial = model.IsRawMaterial;
            product.IsFinishedGood = model.IsFinishedGood;
            product.StandardCost = model.StandardCost;
            product.Barcode = model.Barcode;
            product.Notes = model.Notes;

            _uow.Products.Update(product);
            await _uow.SaveChangesAsync();
            return await GetProductByIdAsync(id);
        }

        // ═══════════════════════════════════════
        //  BOMs
        // ═══════════════════════════════════════

        public async Task<BOMDetailVm> GetBOMByIdAsync(int id)
        {
            var bom = await _uow.BillOfMaterials.Query()
                .Include(b => b.Product)
                .Include(b => b.Lines).ThenInclude(l => l.ComponentProduct)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bom == null) throw new KeyNotFoundException("BOM not found.");
            return MapBomDetail(bom);
        }

        public async Task<IReadOnlyList<BOMListItemVm>> GetBOMsAsync()
        {
            var boms = await _uow.BillOfMaterials.Query()
                .Include(b => b.Product)
                .Include(b => b.Lines)
                .OrderBy(b => b.BOMCode).ToListAsync();
            return boms.Select(b => new BOMListItemVm
            {
                Id = b.Id,
                BOMCode = b.BOMCode,
                ProductName = b.Product.Name,
                Version = b.Version,
                Status = b.Status,
                LineCount = b.Lines.Count
            }).ToList();
        }

        public async Task<BOMDetailVm> CreateBOMAsync(BOMCreateVm model)
        {
            var product = await _uow.Products.GetByIdAsync(model.ProductId);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            var bomCode = await BOMCodeGenerator.GenerateAsync(product.ProductCode, _uow);

            var bom = new BillOfMaterial
            {
                BOMCode = bomCode,
                ProductId = model.ProductId,
                Version = "1.0",
                Status = BOMStatus.Draft,
                OutputQuantity = model.OutputQuantity,
                Notes = model.Notes
            };

            await _uow.BillOfMaterials.AddAsync(bom);
            await _uow.SaveChangesAsync();
            _logger.LogInformation("BOM created: {Code}", bomCode);
            return await GetBOMByIdAsync(bom.Id);
        }

        public async Task<BOMDetailVm> AddBOMLineAsync(BOMLineCreateVm model)
        {
            var bom = await _uow.BillOfMaterials.GetByIdAsync(model.BOMId);
            if (bom == null) throw new KeyNotFoundException("BOM not found.");
            if (bom.Status != BOMStatus.Draft)
                throw new InvalidOperationException("Can only modify draft BOMs.");

            var component = await _uow.Products.GetByIdAsync(model.ComponentProductId);
            if (component == null) throw new KeyNotFoundException("Component product not found.");

            // Prevent self-reference
            if (bom.ProductId == model.ComponentProductId)
                throw new InvalidOperationException("A product cannot be a component of its own BOM.");

            var maxSort = await _uow.BOMLines.Query()
                .Where(l => l.BOMId == model.BOMId).MaxAsync(l => (int?)l.SortOrder) ?? 0;

            var line = new BOMLine
            {
                BOMId = model.BOMId,
                ComponentProductId = model.ComponentProductId,
                Quantity = model.Quantity,
                UOM = model.UOM,
                ScrapFactor = model.ScrapFactor,
                IsPhantom = model.IsPhantom,
                SortOrder = maxSort + 10,
                Notes = model.Notes
            };

            await _uow.BOMLines.AddAsync(line);
            await _uow.SaveChangesAsync();
            return await GetBOMByIdAsync(model.BOMId);
        }

        public async Task RemoveBOMLineAsync(int lineId)
        {
            var line = await _uow.BOMLines.GetByIdAsync(lineId);
            if (line == null) throw new KeyNotFoundException("BOM line not found.");
            _uow.BOMLines.SoftDelete(line);
            await _uow.SaveChangesAsync();
        }

        public async Task<BOMDetailVm> ActivateBOMAsync(int id)
        {
            var bom = await _uow.BillOfMaterials.Query()
                .Include(b => b.Lines).FirstOrDefaultAsync(b => b.Id == id);
            if (bom == null) throw new KeyNotFoundException("BOM not found.");
            if (!bom.Lines.Any())
                throw new InvalidOperationException("Cannot activate a BOM with no lines.");

            bom.Status = BOMStatus.Active;
            bom.EffectiveDate = DateTimeOffset.UtcNow;
            _uow.BillOfMaterials.Update(bom);
            await _uow.SaveChangesAsync();
            return await GetBOMByIdAsync(id);
        }

        // ═══════════════════════════════════════
        //  ROUTINGS
        // ═══════════════════════════════════════

        public async Task<RoutingDetailVm> GetRoutingByIdAsync(int id)
        {
            var routing = await _uow.Routings.Query()
                .Include(r => r.Product)
                .Include(r => r.Operations.OrderBy(o => o.Sequence))
                    .ThenInclude(o => o.WorkCenter)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (routing == null) throw new KeyNotFoundException("Routing not found.");
            return MapRoutingDetail(routing);
        }

        public async Task<IReadOnlyList<RoutingDetailVm>> GetRoutingsAsync()
        {
            var routings = await _uow.Routings.Query()
                .Include(r => r.Product)
                .Include(r => r.Operations).ThenInclude(o => o.WorkCenter)
                .OrderBy(r => r.RoutingCode).ToListAsync();
            return routings.Select(MapRoutingDetail).ToList();
        }

        public async Task<RoutingDetailVm> CreateRoutingAsync(RoutingCreateVm model)
        {
            var product = await _uow.Products.GetByIdAsync(model.ProductId);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            var code = await RoutingCodeGenerator.GenerateAsync(product.ProductCode, _uow);

            var routing = new Routing
            {
                RoutingCode = code,
                ProductId = model.ProductId,
                Version = "1.0",
                Notes = model.Notes
            };

            await _uow.Routings.AddAsync(routing);
            await _uow.SaveChangesAsync();
            _logger.LogInformation("Routing created: {Code}", code);
            return await GetRoutingByIdAsync(routing.Id);
        }

        public async Task<RoutingDetailVm> AddOperationAsync(RoutingOperationCreateVm model)
        {
            var routing = await _uow.Routings.GetByIdAsync(model.RoutingId);
            if (routing == null) throw new KeyNotFoundException("Routing not found.");

            var wc = await _uow.WorkCenters.GetByIdAsync(model.WorkCenterId);
            if (wc == null) throw new KeyNotFoundException("Work center not found.");

            var op = new RoutingOperation
            {
                RoutingId = model.RoutingId,
                Sequence = model.Sequence,
                OperationName = model.OperationName,
                WorkCenterId = model.WorkCenterId,
                SetupTimeMinutes = model.SetupTimeMinutes,
                RunTimePerUnitMinutes = model.RunTimePerUnitMinutes,
                OverlapQuantity = model.OverlapQuantity,
                Instructions = model.Instructions
            };

            await _uow.RoutingOperations.AddAsync(op);

            // Recalculate total cycle time
            var allOps = await _uow.RoutingOperations.Query()
                .Where(o => o.RoutingId == model.RoutingId).ToListAsync();
            routing.TotalCycleTimeMinutes = allOps.Sum(o => o.SetupTimeMinutes + o.RunTimePerUnitMinutes)
                                            + model.SetupTimeMinutes + model.RunTimePerUnitMinutes;
            _uow.Routings.Update(routing);

            await _uow.SaveChangesAsync();
            return await GetRoutingByIdAsync(model.RoutingId);
        }

        public async Task RemoveOperationAsync(int operationId)
        {
            var op = await _uow.RoutingOperations.GetByIdAsync(operationId);
            if (op == null) throw new KeyNotFoundException("Operation not found.");
            _uow.RoutingOperations.SoftDelete(op);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  WORK CENTERS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<WorkCenterListVm>> GetWorkCentersAsync()
        {
            var wcs = await _uow.WorkCenters.Query()
                .Where(w => w.Status == WorkCenterStatus.Active)
                .OrderBy(w => w.Code).ToListAsync();
            return wcs.Select(w => new WorkCenterListVm
            { Id = w.Id, Code = w.Code, Name = w.Name }).ToList();
        }

        // ═══════════════════════════════════════
        //  PRODUCTION ORDERS
        // ═══════════════════════════════════════

        public async Task<ProductionOrderDetailVm> GetOrderByIdAsync(int id)
        {
            var order = await _uow.ProductionOrders.GetWithDetailsAsync(id);
            if (order == null) throw new KeyNotFoundException("Production order not found.");

            var vm = MapOrderDetail(order);
            vm.OEE = await CalculateOEEAsync(id);
            return vm;
        }

        public async Task<ProductionOrderListResultVm> GetOrdersAsync(ProductionOrderFilterVm filter)
        {
            var query = _uow.ProductionOrders.Query()
                .Include(o => o.Product).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(o => o.OrderNumber.Contains(term)
                                         || o.Product.Name.Contains(term));
            }
            if (filter.Status.HasValue) query = query.Where(o => o.Status == filter.Status.Value);
            if (filter.Priority.HasValue) query = query.Where(o => o.Priority == filter.Priority.Value);
            if (filter.ProductId.HasValue) query = query.Where(o => o.ProductId == filter.ProductId.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "product" => filter.SortDescending ? query.OrderByDescending(o => o.Product.Name) : query.OrderBy(o => o.Product.Name),
                "status" => filter.SortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                "priority" => filter.SortDescending ? query.OrderByDescending(o => o.Priority) : query.OrderBy(o => o.Priority),
                "start" => filter.SortDescending ? query.OrderByDescending(o => o.PlannedStartDate) : query.OrderBy(o => o.PlannedStartDate),
                _ => filter.SortDescending ? query.OrderByDescending(o => o.OrderNumber) : query.OrderBy(o => o.OrderNumber)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            return new ProductionOrderListResultVm
            {
                Items = items.Select(o => new ProductionOrderListItemVm
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    ProductName = o.Product.Name,
                    PlannedQuantity = o.PlannedQuantity,
                    CompletedQuantity = o.CompletedQuantity,
                    Status = o.Status,
                    Priority = o.Priority,
                    PlannedStartDate = o.PlannedStartDate
                }).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ProductionOrderDetailVm> CreateOrderAsync(ProductionOrderCreateVm model)
        {
            var product = await _uow.Products.GetByIdAsync(model.ProductId);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            var orderNumber = await ProductionOrderNumberGenerator.GenerateAsync(_uow);

            var order = new ProductionOrder
            {
                OrderNumber = orderNumber,
                ProductId = model.ProductId,
                BOMId = model.BOMId,
                RoutingId = model.RoutingId,
                PlannedQuantity = model.PlannedQuantity,
                Priority = model.Priority,
                Status = ProductionOrderStatus.Draft,
                PlannedStartDate = model.PlannedStartDate,
                PlannedEndDate = model.PlannedEndDate,
                Notes = model.Notes
            };

            await _uow.ProductionOrders.AddAsync(order);
            await _uow.SaveChangesAsync();
            _logger.LogInformation("Production order created: {Number}", orderNumber);
            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<ProductionOrderDetailVm> ChangeOrderStatusAsync(
            int id, ProductionOrderStatus newStatus)
        {
            var order = await _uow.ProductionOrders.GetByIdAsync(id);
            if (order == null) throw new KeyNotFoundException("Production order not found.");

            ValidateStatusTransition(order.Status, newStatus);

            if (newStatus == ProductionOrderStatus.Released && !order.BOMId.HasValue)
                throw new InvalidOperationException("Cannot release an order without a BOM.");

            if (newStatus == ProductionOrderStatus.InProgress && !order.ActualStartDate.HasValue)
                order.ActualStartDate = DateTimeOffset.UtcNow;

            if (newStatus == ProductionOrderStatus.Completed)
                order.ActualEndDate = DateTimeOffset.UtcNow;

            order.Status = newStatus;
            var current = order.Status;
            _uow.ProductionOrders.Update(order);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Order {Number} status → {Status}", order.OrderNumber, newStatus);
            await _workflowHook.OnProductionOrderStatusChangedAsync(id, order.OrderNumber, current.ToString(), newStatus.ToString());
            return await GetOrderByIdAsync(id);
        }

        public async Task<ProductionOrderDetailVm> RecordProductionAsync(ProductionLogCreateVm model)
        {
            var order = await _uow.ProductionOrders.GetByIdAsync(model.ProductionOrderId);
            if (order == null) throw new KeyNotFoundException("Production order not found.");

            if (order.Status != ProductionOrderStatus.InProgress && order.Status != ProductionOrderStatus.Released)
                throw new InvalidOperationException("Can only record production for released or in-progress orders.");

            // Auto-start if released
            if (order.Status == ProductionOrderStatus.Released)
            {
                order.Status = ProductionOrderStatus.InProgress;
                order.ActualStartDate = DateTimeOffset.UtcNow;
            }

            var log = new ProductionLog
            {
                ProductionOrderId = model.ProductionOrderId,
                RoutingOperationId = model.RoutingOperationId,
                WorkCenterId = model.WorkCenterId,
                QuantityProduced = model.QuantityProduced,
                QuantityScrapped = model.QuantityScrapped,
                QuantityRework = model.QuantityRework,
                LogDate = DateTimeOffset.UtcNow,
                SetupTimeActual = model.SetupTimeActual,
                RunTimeActual = model.RunTimeActual,
                DowntimeMinutes = model.DowntimeMinutes,
                Notes = model.Notes
            };

            await _uow.ProductionLogs.AddAsync(log);

            // Update order totals
            order.CompletedQuantity += model.QuantityProduced;
            order.ScrapQuantity += model.QuantityScrapped;
            _uow.ProductionOrders.Update(order);

            await _uow.SaveChangesAsync();

            _logger.LogInformation("Production recorded for {Number}: +{Qty} produced, +{Scrap} scrap",
                order.OrderNumber, model.QuantityProduced, model.QuantityScrapped);

            return await GetOrderByIdAsync(model.ProductionOrderId);
        }

        // ═══════════════════════════════════════
        //  LOT / BATCH
        // ═══════════════════════════════════════

        public async Task<LotBatchListVm> CreateLotBatchAsync(LotBatchCreateVm model)
        {
            var product = await _uow.Products.GetByIdAsync(model.ProductId);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            var lotNumber = await LotNumberGenerator.GenerateAsync(_uow);

            var lot = new LotBatch
            {
                LotNumber = lotNumber,
                ProductId = model.ProductId,
                ProductionOrderId = model.ProductionOrderId,
                Quantity = model.Quantity,
                Status = LotBatchStatus.Created,
                ManufactureDate = DateTimeOffset.UtcNow,
                ExpiryDate = model.ExpiryDate,
                StorageLocation = model.StorageLocation,
                Notes = model.Notes
            };

            await _uow.LotBatches.AddAsync(lot);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Lot created: {LotNumber}", lotNumber);

            return new LotBatchListVm
            {
                Id = lot.Id,
                LotNumber = lot.LotNumber,
                ProductName = product.Name,
                Quantity = lot.Quantity,
                Status = lot.Status,
                ManufactureDate = lot.ManufactureDate,
                ExpiryDate = lot.ExpiryDate,
                StorageLocation = lot.StorageLocation,
                QualityCheckCount = 0
            };
        }

        public async Task ChangeLotStatusAsync(int id, LotBatchStatus newStatus)
        {
            var lot = await _uow.LotBatches.GetByIdAsync(id);
            if (lot == null) throw new KeyNotFoundException("Lot not found.");
            lot.Status = newStatus;
            _uow.LotBatches.Update(lot);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  QUALITY
        // ═══════════════════════════════════════

        public async Task<QualityCheckVm> RecordQualityCheckAsync(QualityCheckCreateVm model)
        {
            var qc = new QualityCheck
            {
                ProductionOrderId = model.ProductionOrderId,
                LotBatchId = model.LotBatchId,
                RoutingOperationId = model.RoutingOperationId,
                CheckName = model.CheckName,
                Specification = model.Specification,
                ActualValue = model.ActualValue,
                Result = model.Result,
                CheckDate = DateTimeOffset.UtcNow,
                Findings = model.Findings,
                CorrectiveAction = model.CorrectiveAction
            };

            await _uow.QualityChecks.AddAsync(qc);
            await _uow.SaveChangesAsync();

            if (model.Result == QualityCheckResult.Fail && model.ProductionOrderId.HasValue)
            {
                var failOrder = await _uow.ProductionOrders.GetByIdAsync(model.ProductionOrderId.Value);
                if (failOrder != null)
                    await _workflowHook.OnQualityCheckFailedAsync(failOrder.Id, failOrder.OrderNumber, model.CheckName);
            }

            // If lot check fails, quarantine the lot
            if (model.LotBatchId.HasValue && model.Result == QualityCheckResult.Fail)
            {
                var lot = await _uow.LotBatches.GetByIdAsync(model.LotBatchId.Value);
                if (lot != null)
                {
                    lot.Status = LotBatchStatus.Quarantine;
                    _uow.LotBatches.Update(lot);
                    await _uow.SaveChangesAsync();
                    await _workflowHook.OnLotQuarantinedAsync(lot.Id, lot.LotNumber, lot.Product?.Name ?? "");

                    _logger.LogWarning("Lot {LotId} quarantined due to failed quality check", model.LotBatchId);
                }
            }

            return new QualityCheckVm
            {
                Id = qc.Id,
                CheckName = qc.CheckName,
                Specification = qc.Specification,
                ActualValue = qc.ActualValue,
                Result = qc.Result,
                CheckDate = qc.CheckDate,
                Findings = qc.Findings,
                CorrectiveAction = qc.CorrectiveAction
            };
        }

        // ═══════════════════════════════════════
        //  OEE
        // ═══════════════════════════════════════

        public async Task<OEEResultVm> CalculateOEEAsync(int productionOrderId)
        {
            var order = await _uow.ProductionOrders.Query()
                .Include(o => o.Routing).ThenInclude(r => r!.Operations)
                .Include(o => o.ProductionLogs)
                .FirstOrDefaultAsync(o => o.Id == productionOrderId);

            if (order == null) throw new KeyNotFoundException("Production order not found.");

            var logs = order.ProductionLogs.ToList();
            if (!logs.Any())
                return new OEEResultVm(); // No data yet

            var totalRunTime = logs.Sum(l => l.RunTimeActual ?? 0);
            var totalSetup = logs.Sum(l => l.SetupTimeActual ?? 0);
            var totalDowntime = logs.Sum(l => l.DowntimeMinutes ?? 0);
            var totalProduced = logs.Sum(l => l.QuantityProduced);
            var totalScrap = logs.Sum(l => l.QuantityScrapped);

            // Planned time = total run + setup + downtime (all recorded time)
            var plannedTime = totalRunTime + totalSetup + totalDowntime;

            // Ideal cycle time from routing (if available)
            var idealCycleTime = order.Routing?.Operations
                .Sum(o => o.RunTimePerUnitMinutes) ?? 0;
            if (idealCycleTime == 0 && totalProduced > 0)
                idealCycleTime = totalRunTime / totalProduced; // fallback

            return OEEEngine.Calculate(
                plannedProductionMinutes: plannedTime,
                actualRunMinutes: totalRunTime,
                totalDowntimeMinutes: totalDowntime + totalSetup,
                idealCycleTimePerUnit: idealCycleTime,
                totalProduced: totalProduced,
                scrapUnits: totalScrap);
        }

        // ═══════════════════════════════════════
        //  STATUS VALIDATION
        // ═══════════════════════════════════════

        private static void ValidateStatusTransition(
            ProductionOrderStatus current, ProductionOrderStatus target)
        {
            var allowed = current switch
            {
                ProductionOrderStatus.Draft => new[] { ProductionOrderStatus.Planned, ProductionOrderStatus.Cancelled },
                ProductionOrderStatus.Planned => new[] { ProductionOrderStatus.Released, ProductionOrderStatus.Cancelled },
                ProductionOrderStatus.Released => new[] { ProductionOrderStatus.InProgress, ProductionOrderStatus.Cancelled },
                ProductionOrderStatus.InProgress => new[] { ProductionOrderStatus.Completed, ProductionOrderStatus.Cancelled },
                ProductionOrderStatus.Completed => new[] { ProductionOrderStatus.Closed },
                _ => Array.Empty<ProductionOrderStatus>()
            };

            if (!allowed.Contains(target))
                throw new InvalidOperationException(
                    $"Cannot transition from {current} to {target}.");
        }

        // ═══════════════════════════════════════
        //  MAPPING
        // ═══════════════════════════════════════

        private static BOMDetailVm MapBomDetail(BillOfMaterial bom)
        {
            var lines = bom.Lines.Select(l => new BOMLineVm
            {
                Id = l.Id,
                ComponentProductId = l.ComponentProductId,
                ComponentCode = l.ComponentProduct.ProductCode,
                ComponentName = l.ComponentProduct.Name,
                Quantity = l.Quantity,
                UOM = l.UOM.ToString(),
                ScrapFactor = l.ScrapFactor,
                IsPhantom = l.IsPhantom,
                LineCost = l.Quantity * (1 + l.ScrapFactor / 100) * l.ComponentProduct.StandardCost
            }).OrderBy(l => l.ComponentCode).ToList();

            return new BOMDetailVm
            {
                Id = bom.Id,
                BOMCode = bom.BOMCode,
                ProductId = bom.ProductId,
                ProductName = bom.Product.Name,
                ProductCode = bom.Product.ProductCode,
                Version = bom.Version,
                Status = bom.Status,
                OutputQuantity = bom.OutputQuantity,
                EffectiveDate = bom.EffectiveDate,
                Notes = bom.Notes,
                Lines = lines,
                TotalMaterialCost = lines.Sum(l => l.LineCost)
            };
        }

        private static RoutingDetailVm MapRoutingDetail(Routing routing) => new()
        {
            Id = routing.Id,
            RoutingCode = routing.RoutingCode,
            ProductId = routing.ProductId,
            ProductName = routing.Product.Name,
            Version = routing.Version,
            IsActive = routing.IsActive,
            TotalCycleTimeMinutes = routing.TotalCycleTimeMinutes,
            Notes = routing.Notes,
            Operations = routing.Operations.OrderBy(o => o.Sequence).Select(o => new RoutingOperationVm
            {
                Id = o.Id,
                Sequence = o.Sequence,
                OperationName = o.OperationName,
                WorkCenterId = o.WorkCenterId,
                WorkCenterName = o.WorkCenter.Name,
                SetupTimeMinutes = o.SetupTimeMinutes,
                RunTimePerUnitMinutes = o.RunTimePerUnitMinutes,
                OverlapQuantity = o.OverlapQuantity,
                Instructions = o.Instructions
            }).ToList()
        };

        private static ProductionOrderDetailVm MapOrderDetail(ProductionOrder order) => new()
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ProductId = order.ProductId,
            ProductName = order.Product.Name,
            ProductCode = order.Product.ProductCode,
            BOMCode = order.BOM?.BOMCode,
            RoutingCode = order.Routing?.RoutingCode,
            PlannedQuantity = order.PlannedQuantity,
            CompletedQuantity = order.CompletedQuantity,
            ScrapQuantity = order.ScrapQuantity,
            Status = order.Status,
            Priority = order.Priority,
            PlannedStartDate = order.PlannedStartDate,
            PlannedEndDate = order.PlannedEndDate,
            ActualStartDate = order.ActualStartDate,
            ActualEndDate = order.ActualEndDate,
            Notes = order.Notes,
            BOMLines = order.BOM?.Lines.Select(l => new BOMLineVm
            {
                Id = l.Id,
                ComponentProductId = l.ComponentProductId,
                ComponentCode = l.ComponentProduct.ProductCode,
                ComponentName = l.ComponentProduct.Name,
                Quantity = l.Quantity * order.PlannedQuantity,
                UOM = l.UOM.ToString(),
                ScrapFactor = l.ScrapFactor,
                IsPhantom = l.IsPhantom,
                LineCost = l.Quantity * order.PlannedQuantity * (1 + l.ScrapFactor / 100) * l.ComponentProduct.StandardCost
            }).ToList() ?? new List<BOMLineVm>(),
            Operations = order.Routing?.Operations.OrderBy(o => o.Sequence).Select(o => new RoutingOperationVm
            {
                Id = o.Id,
                Sequence = o.Sequence,
                OperationName = o.OperationName,
                WorkCenterId = o.WorkCenterId,
                WorkCenterName = o.WorkCenter.Name,
                SetupTimeMinutes = o.SetupTimeMinutes,
                RunTimePerUnitMinutes = o.RunTimePerUnitMinutes,
                OverlapQuantity = o.OverlapQuantity,
                Instructions = o.Instructions
            }).ToList() ?? new List<RoutingOperationVm>(),
            Logs = order.ProductionLogs.OrderByDescending(l => l.LogDate).Select(l => new ProductionLogVm
            {
                Id = l.Id,
                OperationName = l.RoutingOperation?.OperationName,
                WorkCenterName = l.WorkCenter?.Name,
                QuantityProduced = l.QuantityProduced,
                QuantityScrapped = l.QuantityScrapped,
                QuantityRework = l.QuantityRework,
                LogDate = l.LogDate,
                SetupTimeActual = l.SetupTimeActual,
                RunTimeActual = l.RunTimeActual,
                DowntimeMinutes = l.DowntimeMinutes,
                Notes = l.Notes
            }).ToList(),
            LotBatches = order.LotBatches.Select(l => new LotBatchListVm
            {
                Id = l.Id,
                LotNumber = l.LotNumber,
                ProductName = order.Product.Name,
                Quantity = l.Quantity,
                Status = l.Status,
                ManufactureDate = l.ManufactureDate,
                ExpiryDate = l.ExpiryDate,
                StorageLocation = l.StorageLocation,
                QualityCheckCount = l.QualityChecks?.Count ?? 0
            }).ToList(),
            QualityChecks = order.QualityChecks.OrderByDescending(q => q.CheckDate).Select(q => new QualityCheckVm
            {
                Id = q.Id,
                CheckName = q.CheckName,
                Specification = q.Specification,
                ActualValue = q.ActualValue,
                Result = q.Result,
                CheckDate = q.CheckDate,
                Findings = q.Findings,
                CorrectiveAction = q.CorrectiveAction
            }).ToList()
        };

        public async Task<BOMExplosionResultVm> ExplodeBOMAsync(int bomId, decimal quantity)
        {
            var engine = new BOMExplosionEngine(_uow);
            return await engine.ExplodeAsync(bomId, quantity);
        }

        public async Task<ScheduleResultVm> ScheduleOrderAsync(int orderId, DateTimeOffset startDate)
        {
            var scheduler = new ProductionScheduler(_uow);
            return await scheduler.ScheduleOrderAsync(orderId, startDate);
        }

        public async Task<IReadOnlyList<WorkCenterCapacityVm>> GetCapacityOverviewAsync(
            DateTimeOffset from, DateTimeOffset to)
        {
            var scheduler = new ProductionScheduler(_uow);
            return await scheduler.GetCapacityOverviewAsync(from, to);
        }

        public async Task<ProductionCostResultVm> CalculateOrderCostAsync(int orderId)
        {
            var engine = new ProductionCostEngine(_uow);
            return await engine.CalculateAsync(orderId);
        }

        public async Task<ManufacturingDashboardVm> GetDashboardAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var allOrders = await _uow.ProductionOrders.Query()
                .Include(o => o.Product).ToListAsync();
            var allLogs = await _uow.ProductionLogs.GetAllAsync();
            var allQC = await _uow.QualityChecks.GetAllAsync();
            var allLots = await _uow.LotBatches.Query()
                .Include(l => l.Product).ToListAsync();

            var activeOrders = allOrders.Where(o =>
                o.Status == ProductionOrderStatus.InProgress || o.Status == ProductionOrderStatus.Released).ToList();

            var completedThisMonth = allOrders.Count(o =>
                o.Status == ProductionOrderStatus.Completed && o.ActualEndDate >= monthStart);

            var totalPlanned = activeOrders.Sum(o => o.PlannedQuantity);
            var totalCompleted = activeOrders.Sum(o => o.CompletedQuantity);
            var totalScrap = allOrders.Sum(o => o.ScrapQuantity);
            var totalProduced = allOrders.Sum(o => o.CompletedQuantity);
            var scrapRate = totalProduced > 0 ? Math.Round(totalScrap / totalProduced * 100, 1) : 0;

            // Quality
            var qcPass = allQC.Count(q => q.Result == QualityCheckResult.Pass || q.Result == QualityCheckResult.ConditionalPass);
            var qcFail = allQC.Count(q => q.Result == QualityCheckResult.Fail);
            var qcTotal = allQC.Count(q => q.Result != QualityCheckResult.Pending);
            var passRate = qcTotal > 0 ? Math.Round(qcPass * 100m / qcTotal, 1) : 100;

            // Overdue orders
            var overdueOrders = allOrders
                .Where(o => o.PlannedEndDate.HasValue && o.PlannedEndDate < now
                            && (o.Status == ProductionOrderStatus.InProgress || o.Status == ProductionOrderStatus.Released))
                .Select(o => new OverdueOrderVm
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    ProductName = o.Product.Name,
                    PlannedEndDate = o.PlannedEndDate!.Value,
                    DaysOverdue = (int)(now - o.PlannedEndDate!.Value).TotalDays
                }).OrderByDescending(o => o.DaysOverdue).ToList();

            // Quarantined lots
            var quarantined = allLots.Where(l => l.Status == LotBatchStatus.Quarantine)
                .Select(l => new QuarantinedLotVm
                {
                    LotId = l.Id,
                    LotNumber = l.LotNumber,
                    ProductName = l.Product.Name,
                    Quantity = l.Quantity,
                    ManufactureDate = l.ManufactureDate
                }).ToList();

            // Urgent orders
            var urgent = allOrders
                .Where(o => o.Priority == ProductionPriority.Urgent
                            && (o.Status == ProductionOrderStatus.InProgress || o.Status == ProductionOrderStatus.Released))
                .Select(o => new UrgentOrderVm
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    ProductName = o.Product.Name,
                    CompletionPercent = o.PlannedQuantity > 0 ? Math.Round(o.CompletedQuantity / o.PlannedQuantity * 100, 1) : 0,
                    PlannedEndDate = o.PlannedEndDate
                }).ToList();

            // By status
            var byStatus = allOrders.GroupBy(o => o.Status)
                .Select(g => new OrderStatusBreakdownVm
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = allOrders.Count > 0 ? Math.Round(g.Count() * 100m / allOrders.Count, 1) : 0
                }).OrderByDescending(s => s.Count).ToList();

            // By product (top 10)
            var byProduct = allOrders.GroupBy(o => o.Product.Name)
                .Select(g => new ProductionByProductVm
                {
                    ProductName = g.Key,
                    OrderCount = g.Count(),
                    TotalPlanned = g.Sum(o => o.PlannedQuantity),
                    TotalCompleted = g.Sum(o => o.CompletedQuantity),
                    TotalScrap = g.Sum(o => o.ScrapQuantity)
                }).OrderByDescending(p => p.OrderCount).Take(10).ToList();

            // Work center capacity (next 14 days)
            var scheduler = new ProductionScheduler(_uow);
            var wcLoad = await scheduler.GetCapacityOverviewAsync(now, now.AddDays(14));

            // Recent activity
            var recentLogs = allLogs.OrderByDescending(l => l.LogDate).Take(5)
                .Select(l => new RecentProductionActivityVm
                {
                    Icon = "bi-gear",
                    Description = $"Produced {l.QuantityProduced} units (Order #{l.ProductionOrderId})",
                    Date = l.LogDate,
                    OrderId = l.ProductionOrderId
                });
            var recentQC = allQC.OrderByDescending(q => q.CheckDate).Take(5)
                .Select(q => new RecentProductionActivityVm
                {
                    Icon = q.Result == QualityCheckResult.Fail ? "bi-x-circle" : "bi-check-circle",
                    Description = $"QC '{q.CheckName}' — {q.Result}",
                    Date = q.CheckDate,
                    OrderId = q.ProductionOrderId
                });
            var recentActivity = recentLogs.Concat(recentQC)
                .OrderByDescending(a => a.Date).Take(10).ToList();

            // Average OEE (from completed orders with logs)
            var oeeValues = new List<decimal>();
            foreach (var order in allOrders.Where(o => o.Status == ProductionOrderStatus.Completed || o.Status == ProductionOrderStatus.InProgress))
            {
                try
                {
                    var oee = await CalculateOEEAsync(order.Id);
                    if (oee.TotalProduced > 0) oeeValues.Add(oee.OEE);
                }
                catch { /* skip orders without sufficient data */ }
            }
            var avgOEE = oeeValues.Any() ? Math.Round(oeeValues.Average(), 1) : 0;

            return new ManufacturingDashboardVm
            {
                TotalOrders = allOrders.Count,
                InProgressOrders = allOrders.Count(o => o.Status == ProductionOrderStatus.InProgress),
                ReleasedOrders = allOrders.Count(o => o.Status == ProductionOrderStatus.Released),
                CompletedOrdersThisMonth = completedThisMonth,
                TotalPlannedQty = totalPlanned,
                TotalCompletedQty = totalCompleted,
                OverallCompletionPercent = totalPlanned > 0 ? Math.Round(totalCompleted / totalPlanned * 100, 1) : 0,
                AverageOEE = avgOEE,
                TotalQualityChecks = allQC.Count,
                QualityPassCount = qcPass,
                QualityFailCount = qcFail,
                QualityPassRate = passRate,
                ScrapRate = scrapRate,
                OverdueOrders = overdueOrders,
                QuarantinedLots = quarantined,
                UrgentOrders = urgent,
                ByStatus = byStatus,
                ByProduct = byProduct,
                WorkCenterLoad = wcLoad,
                RecentActivity = recentActivity
            };
        }

        public async Task<byte[]> ExportProductionReportAsync()
        {
            var export = new ManufacturingExportService(_uow);
            return await export.ExportProductionReportAsync();
        }

        public async Task<byte[]> ExportQualityReportAsync()
        {
            var export = new ManufacturingExportService(_uow);
            return await export.ExportQualityReportAsync();
        }
    }
}
