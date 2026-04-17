using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class Rfq : BaseEntity
    {
        [Required, MaxLength(50)]
        public string RfqNumber { get; set; } = null!;

        [Required]
        public int VendorId { get; set; }

        public RfqStatus Status { get; set; } = RfqStatus.Draft;

        public DateTimeOffset RfqDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ResponseDeadline { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Converted to PO?
        public int? PurchaseOrderId { get; set; }

        // Navigation
        public Asset.Vendor Vendor { get; set; } = null!;
        public PurchaseOrder? PurchaseOrder { get; set; }
        public ICollection<RfqLine> Lines { get; set; } = new List<RfqLine>();
    }

    public class RfqLine : BaseEntity
    {
        [Required]
        public int RfqId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public decimal RequestedQuantity { get; set; }

        /// <summary>
        /// Vendor's quoted price (filled after response)
        /// </summary>
        public decimal? QuotedUnitPrice { get; set; }

        public int? QuotedLeadTimeDays { get; set; }

        [MaxLength(500)]
        public string? VendorNotes { get; set; }

        public int SortOrder { get; set; }

        // Navigation
        public Rfq Rfq { get; set; } = null!;
    }
}
