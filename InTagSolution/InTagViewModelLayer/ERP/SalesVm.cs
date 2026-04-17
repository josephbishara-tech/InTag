using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.ERP
{
    // ── Quotation ──

    public class QuotationListVm
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public QuotationStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset QuotationDate { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public bool IsExpired => ValidUntil.HasValue && ValidUntil < DateTimeOffset.UtcNow && Status == QuotationStatus.Sent;
        public decimal Total { get; set; }
        public int LineCount { get; set; }
    }

    public class QuotationDetailVm
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = null!;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public QuotationStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset QuotationDate { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public int Version { get; set; }
        public string? SalesTeamName { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public int? SalesOrderId { get; set; }
        public string? SalesOrderNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public IReadOnlyList<QuotationLineVm> Lines { get; set; } = new List<QuotationLineVm>();
    }

    public class QuotationCreateVm
    {
        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Valid Until")]
        public DateTimeOffset? ValidUntil { get; set; }

        [Display(Name = "Sales Team")]
        public int? SalesTeamId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Terms & Conditions")]
        public string? TermsAndConditions { get; set; }
    }

    public class QuotationLineVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductCode { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineTotal { get; set; }
        public decimal LineTax { get; set; }
    }

    public class QuotationLineAddVm
    {
        [Required]
        public int QuotationId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; } = 1;

        [Required, Range(0, double.MaxValue)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        [Display(Name = "Discount %")]
        public decimal DiscountPercent { get; set; }

        [Range(0, 100)]
        [Display(Name = "Tax %")]
        public decimal TaxPercent { get; set; }
    }

    // ── Sales Order ──

    public class SalesOrderListVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public SalesOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public int LineCount { get; set; }
    }

    public class SalesOrderDetailVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerAddress { get; set; }
        public SalesOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public string? CustomerReference { get; set; }
        public string? SalesTeamName { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public int? QuotationId { get; set; }
        public string? QuotationNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public IReadOnlyList<SalesOrderLineVm> Lines { get; set; } = new List<SalesOrderLineVm>();
    }

    public class SalesOrderCreateVm
    {
        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Expected Delivery")]
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }

        [Display(Name = "Sales Team")]
        public int? SalesTeamId { get; set; }

        [MaxLength(100)]
        [Display(Name = "Customer Reference")]
        public string? CustomerReference { get; set; }

        [Display(Name = "Warehouse")]
        public int? WarehouseId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Terms & Conditions")]
        public string? TermsAndConditions { get; set; }
    }

    public class SalesOrderLineVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductCode { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public decimal RemainingToDeliver => Quantity - DeliveredQuantity;
        public decimal RemainingToInvoice => Quantity - InvoicedQuantity;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineTotal { get; set; }
        public decimal LineTax { get; set; }
    }

    public class SalesOrderLineAddVm
    {
        [Required]
        public int SalesOrderId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; } = 1;

        [Required, Range(0, double.MaxValue)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        [Display(Name = "Discount %")]
        public decimal DiscountPercent { get; set; }

        [Range(0, 100)]
        [Display(Name = "Tax %")]
        public decimal TaxPercent { get; set; }
    }

    // ── Pricelist ──

    public class PricelistListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public bool IsDefault { get; set; }
        public int LineCount { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
    }

    // ── Sales Team ──

    public class SalesTeamListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal DefaultCommissionPercent { get; set; }
        public int RuleCount { get; set; }
    }

    // ── Sales Dashboard ──

    public class SalesDashboardVm
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TotalOrders { get; set; }
        public int OpenOrders { get; set; }
        public int TotalQuotations { get; set; }
        public int PendingQuotations { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public IReadOnlyList<TopCustomerVm> TopCustomers { get; set; } = new List<TopCustomerVm>();
        public IReadOnlyList<MonthlySalesVm> MonthlySales { get; set; } = new List<MonthlySalesVm>();
    }

    public class TopCustomerVm
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
    }

    public class MonthlySalesVm
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Period => $"{Year}-{Month:D2}";
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
    }
}
