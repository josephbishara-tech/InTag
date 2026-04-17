using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.ERP;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.ERP;

namespace InTagLogicLayer.ERP
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITenantService _tenantService;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(IUnitOfWork uow, ITenantService tenantService, ILogger<PurchaseService> logger)
        {
            _uow = uow;
            _tenantService = tenantService;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        //  RFQs
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<RfqListVm>> GetRfqsAsync()
        {
            var rfqs = await _uow.Rfqs.Query()
                .Include(r => r.Vendor)
                .Include(r => r.Lines)
                .OrderByDescending(r => r.RfqDate).ToListAsync();

            return rfqs.Select(r => new RfqListVm
            {
                Id = r.Id,
                RfqNumber = r.RfqNumber,
                VendorName = r.Vendor.Name,
                Status = r.Status,
                RfqDate = r.RfqDate,
                ResponseDeadline = r.ResponseDeadline,
                LineCount = r.Lines.Count(l => l.IsActive)
            }).ToList();
        }

        public async Task<RfqDetailVm> GetRfqByIdAsync(int id)
        {
            var r = await _uow.Rfqs.Query()
                .Include(r => r.Vendor)
                .Include(r => r.PurchaseOrder)
                .Include(r => r.Lines.Where(l => l.IsActive).OrderBy(l => l.SortOrder))
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) throw new KeyNotFoundException("RFQ not found.");
            return MapRfqDetail(r);
        }

        public async Task<RfqDetailVm> CreateRfqAsync(RfqCreateVm model)
        {
            var vendor = await _uow.Vendors.GetByIdAsync(model.VendorId);
            if (vendor == null) throw new KeyNotFoundException("Vendor not found.");

            var number = await ErpNumberGenerator.GenerateAsync("RFQ", _uow,
                () => _uow.Rfqs.Query().Select(r => r.RfqNumber));

            var rfq = new Rfq
            {
                RfqNumber = number,
                VendorId = model.VendorId,
                Status = RfqStatus.Draft,
                ResponseDeadline = model.ResponseDeadline,
                Notes = model.Notes
            };

            await _uow.Rfqs.AddAsync(rfq);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("RFQ created: {Number} for vendor {Vendor}", number, vendor.Name);
            return await GetRfqByIdAsync(rfq.Id);
        }

        public async Task AddRfqLineAsync(RfqLineAddVm model)
        {
            var rfq = await _uow.Rfqs.GetByIdAsync(model.RfqId);
            if (rfq == null) throw new KeyNotFoundException("RFQ not found.");
            if (rfq.Status != RfqStatus.Draft)
                throw new InvalidOperationException("Can only add lines to draft RFQs.");

            var maxOrder = await _uow.RfqLines.Query()
                .Where(l => l.RfqId == model.RfqId)
                .MaxAsync(l => (int?)l.SortOrder) ?? 0;

            var line = new RfqLine
            {
                RfqId = model.RfqId,
                ProductId = model.ProductId,
                RequestedQuantity = model.RequestedQuantity,
                SortOrder = maxOrder + 10
            };

            await _uow.RfqLines.AddAsync(line);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveRfqLineAsync(int lineId)
        {
            var line = await _uow.RfqLines.GetByIdAsync(lineId);
            if (line == null) throw new KeyNotFoundException("Line not found.");
            _uow.RfqLines.SoftDelete(line);
            await _uow.SaveChangesAsync();
        }

        public async Task<RfqDetailVm> SendRfqAsync(int id)
        {
            var rfq = await _uow.Rfqs.Query()
                .Include(r => r.Lines.Where(l => l.IsActive))
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rfq == null) throw new KeyNotFoundException("RFQ not found.");
            if (rfq.Status != RfqStatus.Draft)
                throw new InvalidOperationException("Only draft RFQs can be sent.");
            if (!rfq.Lines.Any())
                throw new InvalidOperationException("RFQ has no lines.");

            rfq.Status = RfqStatus.Sent;
            _uow.Rfqs.Update(rfq);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("RFQ {Number} sent to vendor", rfq.RfqNumber);
            return await GetRfqByIdAsync(id);
        }

        public async Task RecordRfqResponseAsync(RfqLineResponseVm model)
        {
            var line = await _uow.RfqLines.GetByIdAsync(model.RfqLineId);
            if (line == null) throw new KeyNotFoundException("RFQ line not found.");

            line.QuotedUnitPrice = model.QuotedUnitPrice;
            line.QuotedLeadTimeDays = model.QuotedLeadTimeDays;
            line.VendorNotes = model.VendorNotes;

            _uow.RfqLines.Update(line);

            // Check if all lines have responses → mark RFQ as Received
            var rfq = await _uow.Rfqs.Query()
                .Include(r => r.Lines.Where(l => l.IsActive))
                .FirstOrDefaultAsync(r => r.Id == line.RfqId);

            if (rfq != null && rfq.Status == RfqStatus.Sent
                && rfq.Lines.All(l => l.QuotedUnitPrice.HasValue))
            {
                rfq.Status = RfqStatus.Received;
                _uow.Rfqs.Update(rfq);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task<PurchaseOrderDetailVm> ConvertRfqToPurchaseOrderAsync(int id)
        {
            var rfq = await _uow.Rfqs.Query()
                .Include(r => r.Vendor)
                .Include(r => r.Lines.Where(l => l.IsActive))
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rfq == null) throw new KeyNotFoundException("RFQ not found.");
            if (rfq.Status != RfqStatus.Received && rfq.Status != RfqStatus.Draft && rfq.Status != RfqStatus.Sent)
                throw new InvalidOperationException("RFQ cannot be converted in current status.");

            var poNumber = await ErpNumberGenerator.GenerateAsync("PO", _uow,
                () => _uow.PurchaseOrders.Query().Select(o => o.OrderNumber));

            var po = new PurchaseOrder
            {
                OrderNumber = poNumber,
                VendorId = rfq.VendorId,
                Status = PurchaseOrderStatus.Draft,
                BuyerUserId = _tenantService.GetCurrentUserId(),
                RfqId = rfq.Id,
                Notes = rfq.Notes
            };

            await _uow.PurchaseOrders.AddAsync(po);
            await _uow.SaveChangesAsync();

            foreach (var rl in rfq.Lines)
            {
                var poLine = new PurchaseOrderLine
                {
                    PurchaseOrderId = po.Id,
                    ProductId = rl.ProductId,
                    Quantity = rl.RequestedQuantity,
                    UnitCost = rl.QuotedUnitPrice ?? 0,
                    SortOrder = rl.SortOrder
                };
                await _uow.PurchaseOrderLines.AddAsync(poLine);
            }

            await RecalculatePurchaseOrderTotalsAsync(po.Id);

            rfq.Status = RfqStatus.Selected;
            rfq.PurchaseOrderId = po.Id;
            _uow.Rfqs.Update(rfq);

            await _uow.SaveChangesAsync();

            _logger.LogInformation("RFQ {RfqNumber} → Purchase Order {PONumber}", rfq.RfqNumber, poNumber);
            return await GetPurchaseOrderByIdAsync(po.Id);
        }

        public async Task CancelRfqAsync(int id)
        {
            var rfq = await _uow.Rfqs.GetByIdAsync(id);
            if (rfq == null) throw new KeyNotFoundException("RFQ not found.");
            rfq.Status = RfqStatus.Cancelled;
            _uow.Rfqs.Update(rfq);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  PURCHASE ORDERS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<PurchaseOrderListVm>> GetPurchaseOrdersAsync()
        {
            var orders = await _uow.PurchaseOrders.Query()
                .Include(o => o.Vendor)
                .Include(o => o.Lines)
                .OrderByDescending(o => o.OrderDate).ToListAsync();

            return orders.Select(o => new PurchaseOrderListVm
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                VendorName = o.Vendor.Name,
                Status = o.Status,
                OrderDate = o.OrderDate,
                ExpectedReceiptDate = o.ExpectedReceiptDate,
                Total = o.Total,
                PaidAmount = o.PaidAmount,
                BalanceDue = o.BalanceDue,
                LineCount = o.Lines.Count(l => l.IsActive)
            }).ToList();
        }

        public async Task<PurchaseOrderDetailVm> GetPurchaseOrderByIdAsync(int id)
        {
            var o = await _uow.PurchaseOrders.Query()
                .Include(o => o.Vendor)
                .Include(o => o.Rfq)
                .Include(o => o.Lines.Where(l => l.IsActive).OrderBy(l => l.SortOrder))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (o == null) throw new KeyNotFoundException("Purchase order not found.");
            return MapPurchaseOrderDetail(o);
        }

        public async Task<PurchaseOrderDetailVm> CreatePurchaseOrderAsync(PurchaseOrderCreateVm model)
        {
            var vendor = await _uow.Vendors.GetByIdAsync(model.VendorId);
            if (vendor == null) throw new KeyNotFoundException("Vendor not found.");

            var number = await ErpNumberGenerator.GenerateAsync("PO", _uow,
                () => _uow.PurchaseOrders.Query().Select(o => o.OrderNumber));

            var order = new PurchaseOrder
            {
                OrderNumber = number,
                VendorId = model.VendorId,
                Status = PurchaseOrderStatus.Draft,
                ExpectedReceiptDate = model.ExpectedReceiptDate,
                BuyerUserId = _tenantService.GetCurrentUserId(),
                VendorReference = model.VendorReference,
                WarehouseId = model.WarehouseId,
                Notes = model.Notes
            };

            await _uow.PurchaseOrders.AddAsync(order);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Purchase order created: {Number} for vendor {Vendor}", number, vendor.Name);
            return await GetPurchaseOrderByIdAsync(order.Id);
        }

        public async Task AddPurchaseOrderLineAsync(PurchaseOrderLineAddVm model)
        {
            var order = await _uow.PurchaseOrders.GetByIdAsync(model.PurchaseOrderId);
            if (order == null) throw new KeyNotFoundException("Purchase order not found.");
            if (order.Status != PurchaseOrderStatus.Draft)
                throw new InvalidOperationException("Can only add lines to draft orders.");

            var maxOrder = await _uow.PurchaseOrderLines.Query()
                .Where(l => l.PurchaseOrderId == model.PurchaseOrderId)
                .MaxAsync(l => (int?)l.SortOrder) ?? 0;

            var line = new PurchaseOrderLine
            {
                PurchaseOrderId = model.PurchaseOrderId,
                ProductId = model.ProductId,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitCost = model.UnitCost,
                TaxPercent = model.TaxPercent,
                ExpectedDate = model.ExpectedDate,
                SortOrder = maxOrder + 10
            };

            await _uow.PurchaseOrderLines.AddAsync(line);
            await RecalculatePurchaseOrderTotalsAsync(model.PurchaseOrderId);
            await _uow.SaveChangesAsync();
        }

        public async Task RemovePurchaseOrderLineAsync(int lineId)
        {
            var line = await _uow.PurchaseOrderLines.GetByIdAsync(lineId);
            if (line == null) throw new KeyNotFoundException("Line not found.");

            _uow.PurchaseOrderLines.SoftDelete(line);
            await RecalculatePurchaseOrderTotalsAsync(line.PurchaseOrderId);
            await _uow.SaveChangesAsync();
        }

        public async Task<PurchaseOrderDetailVm> ConfirmPurchaseOrderAsync(int id)
        {
            var order = await _uow.PurchaseOrders.Query()
                .Include(o => o.Lines.Where(l => l.IsActive))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new KeyNotFoundException("Purchase order not found.");
            if (order.Status != PurchaseOrderStatus.Draft && order.Status != PurchaseOrderStatus.Approved)
                throw new InvalidOperationException("Only draft or approved orders can be confirmed.");
            if (!order.Lines.Any())
                throw new InvalidOperationException("Order has no lines.");

            order.Status = PurchaseOrderStatus.Confirmed;
            _uow.PurchaseOrders.Update(order);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Purchase order {Number} confirmed. Total: {Total}", order.OrderNumber, order.Total);
            return await GetPurchaseOrderByIdAsync(id);
        }

        public async Task CancelPurchaseOrderAsync(int id)
        {
            var order = await _uow.PurchaseOrders.GetByIdAsync(id);
            if (order == null) throw new KeyNotFoundException("Purchase order not found.");
            if (order.Status >= PurchaseOrderStatus.Received)
                throw new InvalidOperationException("Cannot cancel a received order.");

            order.Status = PurchaseOrderStatus.Cancelled;
            _uow.PurchaseOrders.Update(order);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  DASHBOARD
        // ═══════════════════════════════════════

        public async Task<PurchaseDashboardVm> GetDashboardAsync()
        {
            var orders = await _uow.PurchaseOrders.Query()
                .Include(o => o.Vendor)
                .Where(o => o.Status != PurchaseOrderStatus.Cancelled)
                .ToListAsync();

            var rfqs = await _uow.Rfqs.GetAllAsync();
            var confirmedOrders = orders.Where(o => o.Status >= PurchaseOrderStatus.Confirmed).ToList();

            var topVendors = confirmedOrders
                .GroupBy(o => new { o.VendorId, o.Vendor.Name })
                .Select(g => new TopVendorVm
                {
                    VendorId = g.Key.VendorId,
                    VendorName = g.Key.Name,
                    TotalAmount = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderByDescending(v => v.TotalAmount).Take(10).ToList();

            var monthlyPurchases = confirmedOrders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyPurchaseVm
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                .Take(12).ToList();

            return new PurchaseDashboardVm
            {
                TotalSpend = confirmedOrders.Sum(o => o.Total),
                TotalOutstanding = confirmedOrders.Sum(o => o.BalanceDue),
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status < PurchaseOrderStatus.Received && o.Status != PurchaseOrderStatus.Cancelled),
                TotalRfqs = rfqs.Count,
                PendingRfqs = rfqs.Count(r => r.Status == RfqStatus.Draft || r.Status == RfqStatus.Sent),
                TopVendors = topVendors,
                MonthlyPurchases = monthlyPurchases
            };
        }

        // ═══════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════

        private async Task RecalculatePurchaseOrderTotalsAsync(int orderId)
        {
            var lines = await _uow.PurchaseOrderLines.Query()
                .Where(l => l.PurchaseOrderId == orderId && l.IsActive).ToListAsync();

            var order = await _uow.PurchaseOrders.GetByIdAsync(orderId);
            if (order == null) return;

            order.SubTotal = lines.Sum(l => l.LineTotal);
            order.TaxAmount = lines.Sum(l => l.LineTax);
            order.Total = order.SubTotal + order.TaxAmount;

            _uow.PurchaseOrders.Update(order);
        }

        private RfqDetailVm MapRfqDetail(Rfq r) => new()
        {
            Id = r.Id,
            RfqNumber = r.RfqNumber,
            VendorId = r.VendorId,
            VendorName = r.Vendor.Name,
            Status = r.Status,
            RfqDate = r.RfqDate,
            ResponseDeadline = r.ResponseDeadline,
            Notes = r.Notes,
            PurchaseOrderId = r.PurchaseOrderId,
            PurchaseOrderNumber = r.PurchaseOrder?.OrderNumber,
            CreatedDate = r.CreatedDate,
            Lines = r.Lines.Select(l => new RfqLineVm
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = "",
                RequestedQuantity = l.RequestedQuantity,
                QuotedUnitPrice = l.QuotedUnitPrice,
                QuotedLeadTimeDays = l.QuotedLeadTimeDays,
                VendorNotes = l.VendorNotes
            }).ToList()
        };

        private PurchaseOrderDetailVm MapPurchaseOrderDetail(PurchaseOrder o) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            VendorId = o.VendorId,
            VendorName = o.Vendor.Name,
            VendorAddress = o.Vendor.Address,
            Status = o.Status,
            OrderDate = o.OrderDate,
            ExpectedReceiptDate = o.ExpectedReceiptDate,
            VendorReference = o.VendorReference,
            Notes = o.Notes,
            SubTotal = o.SubTotal,
            TaxAmount = o.TaxAmount,
            Total = o.Total,
            PaidAmount = o.PaidAmount,
            BalanceDue = o.BalanceDue,
            RfqId = o.RfqId,
            RfqNumber = o.Rfq?.RfqNumber,
            CreatedDate = o.CreatedDate,
            Lines = o.Lines.Select(l => new PurchaseOrderLineVm
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = "",
                Description = l.Description,
                Quantity = l.Quantity,
                ReceivedQuantity = l.ReceivedQuantity,
                BilledQuantity = l.BilledQuantity,
                UnitCost = l.UnitCost,
                TaxPercent = l.TaxPercent,
                LineTotal = l.LineTotal,
                LineTax = l.LineTax,
                ExpectedDate = l.ExpectedDate
            }).ToList()
        };
    }
}
