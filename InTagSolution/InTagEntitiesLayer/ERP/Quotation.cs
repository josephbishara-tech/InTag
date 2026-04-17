using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class Quotation : BaseEntity
    {
        [Required, MaxLength(50)]
        public string QuotationNumber { get; set; } = null!;

        [Required]
        public int CustomerId { get; set; }

        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

        public DateTimeOffset QuotationDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ValidUntil { get; set; }

        public int Version { get; set; } = 1;

        public Guid? SalespersonUserId { get; set; }

        public int? SalesTeamId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(2000)]
        public string? TermsAndConditions { get; set; }

        // Totals
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }

        // Converted to SO?
        public int? SalesOrderId { get; set; }

        // Navigation
        public Customer Customer { get; set; } = null!;
        public SalesTeam? SalesTeam { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public ICollection<QuotationLine> Lines { get; set; } = new List<QuotationLine>();
    }

    public class QuotationLine : BaseEntity
    {
        [Required]
        public int QuotationId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount percentage (0-100)
        /// </summary>
        public decimal DiscountPercent { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal LineTotal => Math.Round(Quantity * UnitPrice * (1 - DiscountPercent / 100m), 2);

        public decimal LineTax => Math.Round(LineTotal * TaxPercent / 100m, 2);

        public int SortOrder { get; set; }

        // Navigation
        public Quotation Quotation { get; set; } = null!;
    }
}
