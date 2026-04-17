using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class Journal : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public JournalType JournalType { get; set; }

        public int? DefaultDebitAccountId { get; set; }

        public int? DefaultCreditAccountId { get; set; }

        // Navigation
        public Account? DefaultDebitAccount { get; set; }
        public Account? DefaultCreditAccount { get; set; }
    }

    public class JournalEntry : BaseEntity
    {
        [Required, MaxLength(50)]
        public string EntryNumber { get; set; } = null!;

        [Required]
        public int JournalId { get; set; }

        public DateTimeOffset EntryDate { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Reference { get; set; }

        public JournalEntryState State { get; set; } = JournalEntryState.Draft;

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }

        public bool IsBalanced => TotalDebit == TotalCredit;

        // Navigation
        public Journal Journal { get; set; } = null!;
        public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine : BaseEntity
    {
        [Required]
        public int JournalEntryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public int? CostCenterId { get; set; }

        [MaxLength(200)]
        public string? Label { get; set; }

        [MaxLength(100)]
        public string? Reference { get; set; }

        // Navigation
        public JournalEntry JournalEntry { get; set; } = null!;
        public Account Account { get; set; } = null!;
        public CostCenter? CostCenter { get; set; }
    }
}
