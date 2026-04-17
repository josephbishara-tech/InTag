using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class Invoice : BaseEntity
    {
        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = null!;

        [Required]
        public InvoiceType InvoiceType { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? DueDate { get; set; }

        /// <summary>
        /// Customer (for customer invoices/credit notes)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Vendor (for vendor bills/debit notes)
        /// </summary>
        public int? VendorId { get; set; }

        [MaxLength(100)]
        public string? Reference { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Totals
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue => Total - PaidAmount;

        // Source references
        public int? SalesOrderId { get; set; }
        public int? PurchaseOrderId { get; set; }

        // Posted journal entry
        public int? JournalEntryId { get; set; }

        // Navigation
        public Customer? Customer { get; set; }
        public Asset.Vendor? Vendor { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public JournalEntry? JournalEntry { get; set; }
        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class InvoiceLine : BaseEntity
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal LineTotal => Math.Round(Quantity * UnitPrice * (1 - DiscountPercent / 100m), 2);

        public decimal LineTax => Math.Round(LineTotal * TaxPercent / 100m, 2);

        public int? AccountId { get; set; }

        public int SortOrder { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;
        public Account? Account { get; set; }
    }
}
