using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class Payment : BaseEntity
    {
        [Required, MaxLength(50)]
        public string PaymentNumber { get; set; } = null!;

        [Required]
        public int InvoiceId { get; set; }

        public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.UtcNow;

        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [MaxLength(200)]
        public string? Reference { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? JournalEntryId { get; set; }

        public int? BankAccountId { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;
        public JournalEntry? JournalEntry { get; set; }
        public Account? BankAccount { get; set; }
    }
}
