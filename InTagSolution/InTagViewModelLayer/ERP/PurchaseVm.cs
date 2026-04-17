using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.ERP
{
    // ── RFQ ──

    public class RfqListVm
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = null!;
        public string VendorName { get; set; } = null!;
        public RfqStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset RfqDate { get; set; }
        public DateTimeOffset? ResponseDeadline { get; set; }
        public bool IsOverdue => ResponseDeadline.HasValue && ResponseDeadline < DateTimeOffset.UtcNow && Status == RfqStatus.Sent;
        public int LineCount { get; set; }
    }

    public class RfqDetailVm
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = null!;
        public int VendorId { get; set; }
        public string VendorName { get; set; } = null!;
        public RfqStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset RfqDate { get; set; }
        public DateTimeOffset? ResponseDeadline { get; set; }
        public string? Notes { get; set; }
        public int? PurchaseOrderId { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public IReadOnlyList<RfqLineVm> Lines { get; set; } = new List<RfqLineVm>();
    }

    public class RfqCreateVm
    {
        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Response Deadline")]
        public DateTimeOffset? ResponseDeadline { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class RfqLineVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductCode { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal? QuotedUnitPrice { get; set; }
        public int? QuotedLeadTimeDays { get; set; }
        public string? VendorNotes { get; set; }
        public decimal? QuotedTotal => QuotedUnitPrice.HasValue ? QuotedUnitPrice.Value * RequestedQuantity : null;
    }

    public class RfqLineAddVm
    {
        [Required]
        public int RfqId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required, Range(0.0001, double.MaxValue)]
        [Display(Name = "Requested Quantity")]
        public decimal RequestedQuantity { get; set; } = 1;
    }

    public class RfqLineResponseVm
    {
        [Required]
        public int RfqLineId { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Quoted Unit Price")]
        public decimal? QuotedUnitPrice { get; set; }

        [Range(0, 365)]
        [Display(Name = "Lead Time (days)")]
        public int? QuotedLeadTimeDays { get; set; }

        [MaxLength(500)]
        [Display(Name = "Vendor Notes")]
        public string? VendorNotes { get; set; }
    }

    // ── Purchase Order ──

    public class PurchaseOrderListVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string VendorName { get; set; } = null!;
        public PurchaseOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset? ExpectedReceiptDate { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public int LineCount { get; set; }
    }

    public class PurchaseOrderDetailVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int VendorId { get; set; }
        public string VendorName { get; set; } = null!;
        public string? VendorAddress { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset? ExpectedReceiptDate { get; set; }
        public string? VendorReference { get; set; }
        public string? Notes { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public int? RfqId { get; set; }
        public string? RfqNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public IReadOnlyList<PurchaseOrderLineVm> Lines { get; set; } = new List<PurchaseOrderLineVm>();
    }

    public class PurchaseOrderCreateVm
    {
        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Expected Receipt Date")]
        public DateTimeOffset? ExpectedReceiptDate { get; set; }

        [MaxLength(100)]
        [Display(Name = "Vendor Reference")]
        public string? VendorReference { get; set; }

        [Display(Name = "Warehouse")]
        public int? WarehouseId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class PurchaseOrderLineVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductCode { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal BilledQuantity { get; set; }
        public decimal RemainingToReceive => Quantity - ReceivedQuantity;
        public decimal RemainingToBill => Quantity - BilledQuantity;
        public decimal UnitCost { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal LineTotal { get; set; }
        public decimal LineTax { get; set; }
        public DateTimeOffset? ExpectedDate { get; set; }
    }

    public class PurchaseOrderLineAddVm
    {
        [Required]
        public int PurchaseOrderId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; } = 1;

        [Required, Range(0, double.MaxValue)]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [Range(0, 100)]
        [Display(Name = "Tax %")]
        public decimal TaxPercent { get; set; }

        [Display(Name = "Expected Date")]
        public DateTimeOffset? ExpectedDate { get; set; }
    }

    // ── Purchase Dashboard ──

    public class PurchaseDashboardVm
    {
        public decimal TotalSpend { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalRfqs { get; set; }
        public int PendingRfqs { get; set; }
        public IReadOnlyList<TopVendorVm> TopVendors { get; set; } = new List<TopVendorVm>();
        public IReadOnlyList<MonthlyPurchaseVm> MonthlyPurchases { get; set; } = new List<MonthlyPurchaseVm>();
    }

    public class TopVendorVm
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
    }

    public class MonthlyPurchaseVm
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Period => $"{Year}-{Month:D2}";
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
    }
}
