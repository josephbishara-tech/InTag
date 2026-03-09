using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    public class DocumentFilterVm
    {
        public string? SearchTerm { get; set; }
        public DocumentStatus? Status { get; set; }
        public DocumentType? Type { get; set; }
        public DocumentCategory? Category { get; set; }
        public int? DepartmentId { get; set; }
        public bool? OverdueReview { get; set; }
        public string? SortBy { get; set; } = "DocNumber";
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DocumentListResultVm
    {
        public IReadOnlyList<DocumentListItemVm> Items { get; set; } = new List<DocumentListItemVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class DocumentListItemVm
    {
        public int Id { get; set; }
        public string DocNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DocumentType Type { get; set; }
        public DocumentStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string TypeDisplay => Type.ToString();
        public string CurrentVersion { get; set; } = null!;
        public string? DepartmentName { get; set; }
        public DateTimeOffset? NextReviewDate { get; set; }
        public bool IsCheckedOut { get; set; }
        public bool IsOverdueForReview => NextReviewDate.HasValue && NextReviewDate < DateTimeOffset.UtcNow;
    }
}
