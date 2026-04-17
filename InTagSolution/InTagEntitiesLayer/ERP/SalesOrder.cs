using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class SalesOrder : BaseEntity
    {
        [Required, MaxLength(50)]
        public string OrderNumber { get; set; } = null!;

        [Required]
        public int CustomerId { get; set; }

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ExpectedDeliveryDate { get; set; }

        public Guid? SalespersonUserId { get; set; }

        public int? SalesTeamId { get; set; }

        [MaxLength(100)]
        public string? CustomerReference { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(2000)]
        public string? TermsAndConditions { get; set; }

        public int? WarehouseId { get; set; }

        // Totals
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue => Total - PaidAmount;

        // Source
        public int? QuotationId { get; set; }

        // Navigation
        public Customer Customer { get; set; } = null!;
        public SalesTeam? SalesTeam { get; set; }
        public Quotation? Quotation { get; set; }
        public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    }

    public class SalesOrderLine : BaseEntity
    {
        [Required]
        public int SalesOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal DeliveredQuantity { get; set; }

        public decimal InvoicedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal LineTotal => Math.Round(Quantity * UnitPrice * (1 - DiscountPercent / 100m), 2);

        public decimal LineTax => Math.Round(LineTotal * TaxPercent / 100m, 2);

        public int SortOrder { get; set; }

        // Navigation
        public SalesOrder SalesOrder { get; set; } = null!;
    }
}
