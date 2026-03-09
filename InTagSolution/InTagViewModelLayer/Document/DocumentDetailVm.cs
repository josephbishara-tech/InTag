using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    public class DocumentDetailVm
    {
        public int Id { get; set; }
        public string DocNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DocumentType Type { get; set; }
        public DocumentCategory Category { get; set; }
        public DocumentStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string TypeDisplay => Type.ToString();
        public string CategoryDisplay => Category.ToString();
        public string CurrentVersion { get; set; } = null!;
        public DateTimeOffset? EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public ReviewCycle ReviewCycle { get; set; }
        public DateTimeOffset? NextReviewDate { get; set; }
        public bool IsOverdueForReview => NextReviewDate.HasValue && NextReviewDate < DateTimeOffset.UtcNow;

        // Check-in/out
        public bool IsCheckedOut { get; set; }
        public Guid? CheckedOutByUserId { get; set; }
        public DateTimeOffset? CheckedOutDate { get; set; }

        // ISO
        public string? IsoReference { get; set; }
        public string? ConfidentialityLevel { get; set; }
        public string? Tags { get; set; }
        public string? DepartmentName { get; set; }
        public string? Notes { get; set; }

        // Revisions
        public int RevisionCount { get; set; }
        public IReadOnlyList<RevisionListItemVm> Revisions { get; set; } = new List<RevisionListItemVm>();
        public IReadOnlyList<DistributionListItemVm> Distributions { get; set; } = new List<DistributionListItemVm>();

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }

    public class RevisionListItemVm
    {
        public int Id { get; set; }
        public string RevisionNumber { get; set; } = null!;
        public string ChangeDescription { get; set; } = null!;
        public ApprovalAction ApprovalStatus { get; set; }
        public string ApprovalStatusDisplay => ApprovalStatus.ToString();
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public int FileCount { get; set; }
    }

    public class DistributionListItemVm
    {
        public int Id { get; set; }
        public string RecipientName { get; set; } = null!;
        public string Method { get; set; } = null!;
        public DateTimeOffset SentDate { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTimeOffset? AcknowledgedDate { get; set; }
    }
}
