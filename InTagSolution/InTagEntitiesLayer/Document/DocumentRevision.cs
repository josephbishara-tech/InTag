using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    public class DocumentRevision : BaseEntity
    {
        [Required]
        public int DocumentId { get; set; }

        [Required, MaxLength(20)]
        public string RevisionNumber { get; set; } = null!;

        [Required, MaxLength(1000)]
        public string ChangeDescription { get; set; } = null!;

        // ── Review & Approval ────────────────
        public Guid? ReviewerUserId { get; set; }

        public DateTimeOffset? ReviewDate { get; set; }

        [MaxLength(1000)]
        public string? ReviewComments { get; set; }

        public Guid? ApproverUserId { get; set; }

        public DateTimeOffset? ApprovalDate { get; set; }

        [Required]
        public ApprovalAction ApprovalStatus { get; set; } = ApprovalAction.Pending;

        [MaxLength(1000)]
        public string? ApprovalComments { get; set; }

        // ── Electronic Signature ─────────────
        [MaxLength(500)]
        public string? DigitalSignatureData { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // ── Navigation ───────────────────────
        public Document Document { get; set; } = null!;
        public ICollection<DocumentFile> Files { get; set; } = new List<DocumentFile>();
    }
}
