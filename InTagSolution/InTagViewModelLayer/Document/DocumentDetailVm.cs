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
        public string? CheckedOutByName { get; set; }
        public DateTimeOffset? CheckedOutDate { get; set; }

        // ISO
        public string? IsoReference { get; set; }
        public string? ConfidentialityLevel { get; set; }
        public string? Tags { get; set; }
        public string? DepartmentName { get; set; }
        public string? Notes { get; set; }

        // Revisions — now with file details
        public int RevisionCount { get; set; }
        public IReadOnlyList<RevisionDetailVm> Revisions { get; set; } = new List<RevisionDetailVm>();
        public IReadOnlyList<DistributionListItemVm> Distributions { get; set; } = new List<DistributionListItemVm>();

        // Computed — latest revision files for quick download
        public IReadOnlyList<RevisionFileVm> LatestFiles =>
            Revisions.FirstOrDefault()?.Files ?? new List<RevisionFileVm>();

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }

    public class RevisionDetailVm
    {
        public int Id { get; set; }
        public string RevisionNumber { get; set; } = null!;
        public string? ChangeDescription { get; set; }
        public ApprovalAction ApprovalStatus { get; set; }
        public string ApprovalStatusDisplay => ApprovalStatus.ToString();
        public DateTimeOffset? ApprovalDate { get; set; }
        public string? ApprovalComments { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public int FileCount => Files.Count;
        public IReadOnlyList<RevisionFileVm> Files { get; set; } = new List<RevisionFileVm>();
    }

    public class RevisionFileVm
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileSizeDisplay => FileSize switch
        {
            < 1024 => $"{FileSize} B",
            < 1024 * 1024 => $"{FileSize / 1024} KB",
            _ => $"{FileSize / 1024 / 1024:N1} MB"
        };
        public string FileIcon => FileType?.ToUpper() switch
        {
            "PDF" => "bi-file-earmark-pdf",
            "DOC" or "DOCX" => "bi-file-earmark-word",
            "XLS" or "XLSX" => "bi-file-earmark-excel",
            "PPT" or "PPTX" => "bi-file-earmark-ppt",
            "PNG" or "JPG" or "JPEG" or "GIF" => "bi-file-earmark-image",
            "DWG" or "DXF" => "bi-file-earmark-ruled",
            "ZIP" or "RAR" => "bi-file-earmark-zip",
            _ => "bi-file-earmark"
        };
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
