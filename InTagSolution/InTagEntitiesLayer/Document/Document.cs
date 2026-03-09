using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    public class Document : BaseEntity
    {
        [Required, MaxLength(50)]
        public string DocNumber { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DocumentType Type { get; set; }

        [Required]
        public DocumentCategory Category { get; set; }

        [Required]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        [Required, MaxLength(20)]
        public string CurrentVersion { get; set; } = "1.0";

        public DateTimeOffset? EffectiveDate { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        [Required]
        public ReviewCycle ReviewCycle { get; set; } = ReviewCycle.Annual;

        public DateTimeOffset? NextReviewDate { get; set; }

        // ── Check-in / Check-out ─────────────
        public bool IsCheckedOut { get; set; }

        public Guid? CheckedOutByUserId { get; set; }

        public DateTimeOffset? CheckedOutDate { get; set; }

        // ── ISO Compliance ───────────────────
        [MaxLength(200)]
        public string? IsoReference { get; set; }

        [MaxLength(100)]
        public string? ConfidentialityLevel { get; set; }

        // ── Ownership ────────────────────────
        [Required]
        public Guid AuthorUserId { get; set; }

        public Guid? OwnerUserId { get; set; }

        public int? DepartmentId { get; set; }

        // ── Tags ─────────────────────────────
        /// <summary>
        /// Comma-separated tags for filtering and search
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // ── Navigation ───────────────────────
        public Asset.Department? Department { get; set; }
        public ICollection<DocumentRevision> Revisions { get; set; } = new List<DocumentRevision>();
        public ICollection<DistributionRecord> DistributionRecords { get; set; } = new List<DistributionRecord>();
    }
}
