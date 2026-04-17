using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class PurchaseOrder : BaseEntity
    {
        [Required, MaxLength(50)]
        public string OrderNumber { get; set; } = null!;

        [Required]
        public int VendorId { get; set; }

        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ExpectedReceiptDate { get; set; }

        public Guid? BuyerUserId { get; set; }

        [MaxLength(100)]
        public string? VendorReference { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public int? WarehouseId { get; set; }

        // Totals
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue => Total - PaidAmount;

        // Source
        public int? RfqId { get; set; }

        // Navigation
        public Asset.Vendor Vendor { get; set; } = null!;
        public Rfq? Rfq { get; set; }
        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    }

    public class PurchaseOrderLine : BaseEntity
    {
        [Required]
        public int PurchaseOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public decimal BilledQuantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal LineTotal => Math.Round(Quantity * UnitCost, 2);

        public decimal LineTax => Math.Round(LineTotal * TaxPercent / 100m, 2);

        public DateTimeOffset? ExpectedDate { get; set; }

        public int SortOrder { get; set; }

        // Navigation
        public PurchaseOrder PurchaseOrder { get; set; } = null!;
    }
}
