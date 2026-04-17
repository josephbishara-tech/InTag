using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.ERP;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.ERP;

namespace InTagLogicLayer.ERP
{
    public class SalesService : ISalesService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITenantService _tenantService;
        private readonly ILogger<SalesService> _logger;

        public SalesService(IUnitOfWork uow, ITenantService tenantService, ILogger<SalesService> logger)
        {
            _uow = uow;
            _tenantService = tenantService;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        //  CUSTOMERS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<CustomerListVm>> GetCustomersAsync()
        {
            var customers = await _uow.Customers.Query()
                .Include(c => c.SalesOrders)
                .OrderBy(c => c.Name).ToListAsync();

            return customers.Select(c => new CustomerListVm
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ContactPerson = c.ContactPerson,
                Email = c.Email,
                Phone = c.Phone,
                City = c.City,
                Country = c.Country,
                Currency = c.Currency,
                CreditLimit = c.CreditLimit,
                OrderCount = c.SalesOrders.Count(o => o.IsActive),
                TotalRevenue = c.SalesOrders.Where(o => o.IsActive && o.Status >= SalesOrderStatus.Confirmed)
                    .Sum(o => o.Total)
            }).ToList();
        }

        public async Task<CustomerDetailVm> GetCustomerByIdAsync(int id)
        {
            var c = await _uow.Customers.Query()
                .Include(c => c.Pricelist)
                .Include(c => c.SalesTeam)
                .Include(c => c.SalesOrders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) throw new KeyNotFoundException("Customer not found.");

            return new CustomerDetailVm
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ContactPerson = c.ContactPerson,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                TaxId = c.TaxId,
                CreditLimit = c.CreditLimit,
                PaymentTermDays = c.PaymentTermDays,
                Currency = c.Currency,
                PricelistName = c.Pricelist?.Name,
                SalesTeamName = c.SalesTeam?.Name,
                Notes = c.Notes,
                CreatedDate = c.CreatedDate,
                OrderCount = c.SalesOrders.Count(o => o.IsActive),
                TotalRevenue = c.SalesOrders.Where(o => o.IsActive && o.Status >= SalesOrderStatus.Confirmed)
                    .Sum(o => o.Total)
            };
        }

        public async Task<CustomerDetailVm> CreateCustomerAsync(CustomerCreateVm model)
        {
            if (await _uow.Customers.ExistsAsync(c => c.Code == model.Code))
                throw new InvalidOperationException($"Customer code '{model.Code}' already exists.");

            var customer = new Customer
            {
                Code = model.Code,
                Name = model.Name,
                ContactPerson = model.ContactPerson,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                TaxId = model.TaxId,
                CreditLimit = model.CreditLimit,
                PaymentTermDays = model.PaymentTermDays,
                Currency = model.Currency,
                PricelistId = model.PricelistId,
                SalesTeamId = model.SalesTeamId,
                Notes = model.Notes
            };

            await _uow.Customers.AddAsync(customer);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Customer created: {Code} — {Name}", customer.Code, customer.Name);
            return await GetCustomerByIdAsync(customer.Id);
        }

        public async Task UpdateCustomerAsync(CustomerUpdateVm model)
        {
            var c = await _uow.Customers.GetByIdAsync(model.Id);
            if (c == null) throw new KeyNotFoundException("Customer not found.");

            if (await _uow.Customers.ExistsAsync(x => x.Code == model.Code && x.Id != model.Id))
                throw new InvalidOperationException($"Customer code '{model.Code}' already exists.");

            c.Code = model.Code; c.Name = model.Name;
            c.ContactPerson = model.ContactPerson; c.Email = model.Email;
            c.Phone = model.Phone; c.Address = model.Address;
            c.City = model.City; c.Country = model.Country;
            c.TaxId = model.TaxId; c.CreditLimit = model.CreditLimit;
            c.PaymentTermDays = model.PaymentTermDays; c.Currency = model.Currency;
            c.PricelistId = model.PricelistId; c.SalesTeamId = model.SalesTeamId;
            c.Notes = model.Notes;

            _uow.Customers.Update(c);
            await _uow.SaveChangesAsync();
            _logger.LogInformation("Customer updated: {Code}", c.Code);
        }

        // ═══════════════════════════════════════
        //  QUOTATIONS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<QuotationListVm>> GetQuotationsAsync()
        {
            var quotes = await _uow.Quotations.Query()
                .Include(q => q.Customer)
                .Include(q => q.Lines)
                .OrderByDescending(q => q.QuotationDate).ToListAsync();

            return quotes.Select(q => new QuotationListVm
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                CustomerName = q.Customer.Name,
                Status = q.Status,
                QuotationDate = q.QuotationDate,
                ValidUntil = q.ValidUntil,
                Total = q.Total,
                LineCount = q.Lines.Count(l => l.IsActive)
            }).ToList();
        }

        public async Task<QuotationDetailVm> GetQuotationByIdAsync(int id)
        {
            var q = await _uow.Quotations.Query()
                .Include(q => q.Customer)
                .Include(q => q.SalesTeam)
                .Include(q => q.SalesOrder)
                .Include(q => q.Lines.Where(l => l.IsActive).OrderBy(l => l.SortOrder))
                .FirstOrDefaultAsync(q => q.Id == id);

            if (q == null) throw new KeyNotFoundException("Quotation not found.");
            return MapQuotationDetail(q);
        }

        public async Task<QuotationDetailVm> CreateQuotationAsync(QuotationCreateVm model)
        {
            var customer = await _uow.Customers.GetByIdAsync(model.CustomerId);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            var number = await ErpNumberGenerator.GenerateAsync("QTN", _uow,
                () => _uow.Quotations.Query().Select(q => q.QuotationNumber));

            var quotation = new Quotation
            {
                QuotationNumber = number,
                CustomerId = model.CustomerId,
                Status = QuotationStatus.Draft,
                QuotationDate = DateTimeOffset.UtcNow,
                ValidUntil = model.ValidUntil,
                SalespersonUserId = _tenantService.GetCurrentUserId(),
                SalesTeamId = model.SalesTeamId ?? customer.SalesTeamId,
                Notes = model.Notes,
                TermsAndConditions = model.TermsAndConditions
            };

            await _uow.Quotations.AddAsync(quotation);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Quotation created: {Number} for customer {Customer}",
                number, customer.Name);
            return await GetQuotationByIdAsync(quotation.Id);
        }

        public async Task AddQuotationLineAsync(QuotationLineAddVm model)
        {
            var quotation = await _uow.Quotations.GetByIdAsync(model.QuotationId);
            if (quotation == null) throw new KeyNotFoundException("Quotation not found.");
            if (quotation.Status != QuotationStatus.Draft)
                throw new InvalidOperationException("Can only add lines to draft quotations.");

            var maxOrder = await _uow.QuotationLines.Query()
                .Where(l => l.QuotationId == model.QuotationId)
                .MaxAsync(l => (int?)l.SortOrder) ?? 0;

            var line = new QuotationLine
            {
                QuotationId = model.QuotationId,
                ProductId = model.ProductId,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitPrice = model.UnitPrice,
                DiscountPercent = model.DiscountPercent,
                TaxPercent = model.TaxPercent,
                SortOrder = maxOrder + 10
            };

            await _uow.QuotationLines.AddAsync(line);
            await RecalculateQuotationTotalsAsync(model.QuotationId);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveQuotationLineAsync(int lineId)
        {
            var line = await _uow.QuotationLines.GetByIdAsync(lineId);
            if (line == null) throw new KeyNotFoundException("Line not found.");

            _uow.QuotationLines.SoftDelete(line);
            await RecalculateQuotationTotalsAsync(line.QuotationId);
            await _uow.SaveChangesAsync();
        }

        public async Task<QuotationDetailVm> SendQuotationAsync(int id)
        {
            var q = await _uow.Quotations.GetByIdAsync(id);
            if (q == null) throw new KeyNotFoundException("Quotation not found.");
            if (q.Status != QuotationStatus.Draft)
                throw new InvalidOperationException("Only draft quotations can be sent.");

            q.Status = QuotationStatus.Sent;
            _uow.Quotations.Update(q);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Quotation {Number} sent", q.QuotationNumber);
            return await GetQuotationByIdAsync(id);
        }

        public async Task<SalesOrderDetailVm> ConfirmQuotationAsync(int id)
        {
            var q = await _uow.Quotations.Query()
                .Include(q => q.Lines.Where(l => l.IsActive))
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (q == null) throw new KeyNotFoundException("Quotation not found.");
            if (q.Status != QuotationStatus.Draft && q.Status != QuotationStatus.Sent)
                throw new InvalidOperationException("Quotation cannot be confirmed in current status.");
            if (!q.Lines.Any())
                throw new InvalidOperationException("Quotation has no lines.");

            // Create Sales Order from Quotation
            var soNumber = await ErpNumberGenerator.GenerateAsync("SO", _uow,
                () => _uow.SalesOrders.Query().Select(o => o.OrderNumber));

            var so = new SalesOrder
            {
                OrderNumber = soNumber,
                CustomerId = q.CustomerId,
                Status = SalesOrderStatus.Draft,
                OrderDate = DateTimeOffset.UtcNow,
                SalespersonUserId = q.SalespersonUserId,
                SalesTeamId = q.SalesTeamId,
                Notes = q.Notes,
                TermsAndConditions = q.TermsAndConditions,
                QuotationId = q.Id,
                SubTotal = q.SubTotal,
                DiscountAmount = q.DiscountAmount,
                TaxAmount = q.TaxAmount,
                Total = q.Total
            };

            await _uow.SalesOrders.AddAsync(so);
            await _uow.SaveChangesAsync();

            // Copy lines
            foreach (var ql in q.Lines)
            {
                var soLine = new SalesOrderLine
                {
                    SalesOrderId = so.Id,
                    ProductId = ql.ProductId,
                    Description = ql.Description,
                    Quantity = ql.Quantity,
                    UnitPrice = ql.UnitPrice,
                    DiscountPercent = ql.DiscountPercent,
                    TaxPercent = ql.TaxPercent,
                    SortOrder = ql.SortOrder
                };
                await _uow.SalesOrderLines.AddAsync(soLine);
            }

            q.Status = QuotationStatus.Confirmed;
            q.SalesOrderId = so.Id;
            _uow.Quotations.Update(q);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Quotation {QNumber} confirmed → Sales Order {SONumber}",
                q.QuotationNumber, soNumber);
            return await GetSalesOrderByIdAsync(so.Id);
        }

        public async Task CancelQuotationAsync(int id)
        {
            var q = await _uow.Quotations.GetByIdAsync(id);
            if (q == null) throw new KeyNotFoundException("Quotation not found.");
            q.Status = QuotationStatus.Cancelled;
            _uow.Quotations.Update(q);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  SALES ORDERS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<SalesOrderListVm>> GetSalesOrdersAsync()
        {
            var orders = await _uow.SalesOrders.Query()
                .Include(o => o.Customer)
                .Include(o => o.Lines)
                .OrderByDescending(o => o.OrderDate).ToListAsync();

            return orders.Select(o => new SalesOrderListVm
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer.Name,
                Status = o.Status,
                OrderDate = o.OrderDate,
                ExpectedDeliveryDate = o.ExpectedDeliveryDate,
                Total = o.Total,
                PaidAmount = o.PaidAmount,
                BalanceDue = o.BalanceDue,
                LineCount = o.Lines.Count(l => l.IsActive)
            }).ToList();
        }

        public async Task<SalesOrderDetailVm> GetSalesOrderByIdAsync(int id)
        {
            var o = await _uow.SalesOrders.Query()
                .Include(o => o.Customer)
                .Include(o => o.SalesTeam)
                .Include(o => o.Quotation)
                .Include(o => o.Lines.Where(l => l.IsActive).OrderBy(l => l.SortOrder))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (o == null) throw new KeyNotFoundException("Sales order not found.");
            return MapSalesOrderDetail(o);
        }

        public async Task<SalesOrderDetailVm> CreateSalesOrderAsync(SalesOrderCreateVm model)
        {
            var customer = await _uow.Customers.GetByIdAsync(model.CustomerId);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            var number = await ErpNumberGenerator.GenerateAsync("SO", _uow,
                () => _uow.SalesOrders.Query().Select(o => o.OrderNumber));

            var order = new SalesOrder
            {
                OrderNumber = number,
                CustomerId = model.CustomerId,
                Status = SalesOrderStatus.Draft,
                OrderDate = DateTimeOffset.UtcNow,
                ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                SalespersonUserId = _tenantService.GetCurrentUserId(),
                SalesTeamId = model.SalesTeamId ?? customer.SalesTeamId,
                CustomerReference = model.CustomerReference,
                WarehouseId = model.WarehouseId,
                Notes = model.Notes,
                TermsAndConditions = model.TermsAndConditions
            };

            await _uow.SalesOrders.AddAsync(order);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Sales order created: {Number} for {Customer}", number, customer.Name);
            return await GetSalesOrderByIdAsync(order.Id);
        }

        public async Task AddSalesOrderLineAsync(SalesOrderLineAddVm model)
        {
            var order = await _uow.SalesOrders.GetByIdAsync(model.SalesOrderId);
            if (order == null) throw new KeyNotFoundException("Sales order not found.");
            if (order.Status != SalesOrderStatus.Draft)
                throw new InvalidOperationException("Can only add lines to draft orders.");

            var maxOrder = await _uow.SalesOrderLines.Query()
                .Where(l => l.SalesOrderId == model.SalesOrderId)
                .MaxAsync(l => (int?)l.SortOrder) ?? 0;

            var line = new SalesOrderLine
            {
                SalesOrderId = model.SalesOrderId,
                ProductId = model.ProductId,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitPrice = model.UnitPrice,
                DiscountPercent = model.DiscountPercent,
                TaxPercent = model.TaxPercent,
                SortOrder = maxOrder + 10
            };

            await _uow.SalesOrderLines.AddAsync(line);
            await RecalculateSalesOrderTotalsAsync(model.SalesOrderId);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveSalesOrderLineAsync(int lineId)
        {
            var line = await _uow.SalesOrderLines.GetByIdAsync(lineId);
            if (line == null) throw new KeyNotFoundException("Line not found.");

            _uow.SalesOrderLines.SoftDelete(line);
            await RecalculateSalesOrderTotalsAsync(line.SalesOrderId);
            await _uow.SaveChangesAsync();
        }

        public async Task<SalesOrderDetailVm> ConfirmSalesOrderAsync(int id)
        {
            var order = await _uow.SalesOrders.Query()
                .Include(o => o.Lines.Where(l => l.IsActive))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new KeyNotFoundException("Sales order not found.");
            if (order.Status != SalesOrderStatus.Draft)
                throw new InvalidOperationException("Only draft orders can be confirmed.");
            if (!order.Lines.Any())
                throw new InvalidOperationException("Order has no lines.");

            order.Status = SalesOrderStatus.Confirmed;
            _uow.SalesOrders.Update(order);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Sales order {Number} confirmed. Total: {Total}", order.OrderNumber, order.Total);
            return await GetSalesOrderByIdAsync(id);
        }

        public async Task CancelSalesOrderAsync(int id)
        {
            var order = await _uow.SalesOrders.GetByIdAsync(id);
            if (order == null) throw new KeyNotFoundException("Sales order not found.");
            if (order.Status >= SalesOrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel a delivered order.");

            order.Status = SalesOrderStatus.Cancelled;
            _uow.SalesOrders.Update(order);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  PRICELISTS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<PricelistListVm>> GetPricelistsAsync()
        {
            var lists = await _uow.Pricelists.Query()
                .Include(p => p.Lines)
                .OrderBy(p => p.Name).ToListAsync();

            return lists.Select(p => new PricelistListVm
            {
                Id = p.Id,
                Name = p.Name,
                Currency = p.Currency,
                IsDefault = p.IsDefault,
                LineCount = p.Lines.Count(l => l.IsActive),
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo
            }).ToList();
        }

        public async Task<decimal?> GetProductPriceAsync(int productId, int? customerId, decimal quantity)
        {
            Pricelist? pricelist = null;

            if (customerId.HasValue)
            {
                var customer = await _uow.Customers.Query()
                    .Include(c => c.Pricelist).ThenInclude(p => p!.Lines)
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                pricelist = customer?.Pricelist;
            }

            if (pricelist == null)
            {
                pricelist = await _uow.Pricelists.Query()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.IsDefault);
            }

            if (pricelist == null) return null;

            var now = DateTimeOffset.UtcNow;
            var line = pricelist.Lines
                .Where(l => l.IsActive && l.ProductId == productId
                    && l.MinQuantity <= quantity
                    && (!l.ValidFrom.HasValue || l.ValidFrom <= now)
                    && (!l.ValidTo.HasValue || l.ValidTo >= now))
                .OrderByDescending(l => l.MinQuantity)
                .FirstOrDefault();

            return line?.UnitPrice;
        }

        // ═══════════════════════════════════════
        //  SALES TEAMS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<SalesTeamListVm>> GetSalesTeamsAsync()
        {
            var teams = await _uow.SalesTeams.Query()
                .Include(t => t.CommissionRules)
                .OrderBy(t => t.Name).ToListAsync();

            return teams.Select(t => new SalesTeamListVm
            {
                Id = t.Id,
                Name = t.Name,
                DefaultCommissionPercent = t.DefaultCommissionPercent,
                RuleCount = t.CommissionRules.Count(r => r.IsActive)
            }).ToList();
        }

        // ═══════════════════════════════════════
        //  DASHBOARD
        // ═══════════════════════════════════════

        public async Task<SalesDashboardVm> GetDashboardAsync()
        {
            var orders = await _uow.SalesOrders.Query()
                .Include(o => o.Customer)
                .Where(o => o.Status != SalesOrderStatus.Cancelled)
                .ToListAsync();

            var quotations = await _uow.Quotations.GetAllAsync();

            var confirmedOrders = orders.Where(o => o.Status >= SalesOrderStatus.Confirmed).ToList();
            var totalQuotations = quotations.Count;
            var confirmedQuotations = quotations.Count(q => q.Status == QuotationStatus.Confirmed);

            var topCustomers = confirmedOrders
                .GroupBy(o => new { o.CustomerId, o.Customer.Name })
                .Select(g => new TopCustomerVm
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.Name,
                    TotalAmount = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount).Take(10).ToList();

            var monthlySales = confirmedOrders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlySalesVm
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                .Take(12).ToList();

            return new SalesDashboardVm
            {
                TotalRevenue = confirmedOrders.Sum(o => o.Total),
                TotalOutstanding = confirmedOrders.Sum(o => o.BalanceDue),
                TotalOrders = orders.Count,
                OpenOrders = orders.Count(o => o.Status < SalesOrderStatus.Done && o.Status != SalesOrderStatus.Cancelled),
                TotalQuotations = totalQuotations,
                PendingQuotations = quotations.Count(q => q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Sent),
                ConversionRate = totalQuotations > 0 ? Math.Round(confirmedQuotations * 100m / totalQuotations, 1) : 0,
                AverageOrderValue = confirmedOrders.Any() ? Math.Round(confirmedOrders.Average(o => o.Total), 2) : 0,
                TopCustomers = topCustomers,
                MonthlySales = monthlySales
            };
        }

        // ═══════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════

        private async Task RecalculateQuotationTotalsAsync(int quotationId)
        {
            var lines = await _uow.QuotationLines.Query()
                .Where(l => l.QuotationId == quotationId && l.IsActive).ToListAsync();

            var quotation = await _uow.Quotations.GetByIdAsync(quotationId);
            if (quotation == null) return;

            quotation.SubTotal = lines.Sum(l => l.LineTotal);
            quotation.TaxAmount = lines.Sum(l => l.LineTax);
            quotation.DiscountAmount = lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100m);
            quotation.Total = quotation.SubTotal + quotation.TaxAmount;

            _uow.Quotations.Update(quotation);
        }

        private async Task RecalculateSalesOrderTotalsAsync(int orderId)
        {
            var lines = await _uow.SalesOrderLines.Query()
                .Where(l => l.SalesOrderId == orderId && l.IsActive).ToListAsync();

            var order = await _uow.SalesOrders.GetByIdAsync(orderId);
            if (order == null) return;

            order.SubTotal = lines.Sum(l => l.LineTotal);
            order.TaxAmount = lines.Sum(l => l.LineTax);
            order.DiscountAmount = lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100m);
            order.Total = order.SubTotal + order.TaxAmount;

            _uow.SalesOrders.Update(order);
        }

        private QuotationDetailVm MapQuotationDetail(Quotation q) => new()
        {
            Id = q.Id,
            QuotationNumber = q.QuotationNumber,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer.Name,
            Status = q.Status,
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            Version = q.Version,
            SalesTeamName = q.SalesTeam?.Name,
            Notes = q.Notes,
            TermsAndConditions = q.TermsAndConditions,
            SubTotal = q.SubTotal,
            DiscountAmount = q.DiscountAmount,
            TaxAmount = q.TaxAmount,
            Total = q.Total,
            SalesOrderId = q.SalesOrderId,
            SalesOrderNumber = q.SalesOrder?.OrderNumber,
            CreatedDate = q.CreatedDate,
            Lines = q.Lines.Select(l => new QuotationLineVm
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = "", // Will be populated via join in controller if needed
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                TaxPercent = l.TaxPercent,
                LineTotal = l.LineTotal,
                LineTax = l.LineTax
            }).ToList()
        };

        private SalesOrderDetailVm MapSalesOrderDetail(SalesOrder o) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer.Name,
            CustomerAddress = o.Customer.Address,
            Status = o.Status,
            OrderDate = o.OrderDate,
            ExpectedDeliveryDate = o.ExpectedDeliveryDate,
            CustomerReference = o.CustomerReference,
            SalesTeamName = o.SalesTeam?.Name,
            Notes = o.Notes,
            TermsAndConditions = o.TermsAndConditions,
            SubTotal = o.SubTotal,
            DiscountAmount = o.DiscountAmount,
            TaxAmount = o.TaxAmount,
            Total = o.Total,
            PaidAmount = o.PaidAmount,
            BalanceDue = o.BalanceDue,
            QuotationId = o.QuotationId,
            QuotationNumber = o.Quotation?.QuotationNumber,
            CreatedDate = o.CreatedDate,
            Lines = o.Lines.Select(l => new SalesOrderLineVm
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = "",
                Description = l.Description,
                Quantity = l.Quantity,
                DeliveredQuantity = l.DeliveredQuantity,
                InvoicedQuantity = l.InvoicedQuantity,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                TaxPercent = l.TaxPercent,
                LineTotal = l.LineTotal,
                LineTax = l.LineTax
            }).ToList()
        };
    }
}
