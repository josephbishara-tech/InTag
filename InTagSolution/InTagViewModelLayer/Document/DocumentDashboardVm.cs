using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    public class DocumentDashboardVm
    {
        // KPIs
        public int TotalDocuments { get; set; }
        public int PublishedDocuments { get; set; }
        public int DraftDocuments { get; set; }
        public int InReviewDocuments { get; set; }
        public int ObsoleteDocuments { get; set; }
        public int CheckedOutDocuments { get; set; }

        // Compliance
        public int OverdueReviewCount { get; set; }
        public int UpcomingReviewCount { get; set; }
        public int PendingApprovalCount { get; set; }
        public int UnacknowledgedDistributionCount { get; set; }
        public decimal ReviewCompliancePercent { get; set; }

        // Alerts
        public IReadOnlyList<ReviewAlertVm> OverdueReviews { get; set; } = new List<ReviewAlertVm>();
        public IReadOnlyList<ReviewAlertVm> UpcomingReviews { get; set; } = new List<ReviewAlertVm>();
        public IReadOnlyList<PendingApprovalVm> PendingApprovals { get; set; } = new List<PendingApprovalVm>();
        public IReadOnlyList<CheckedOutDocVm> CheckedOutDocs { get; set; } = new List<CheckedOutDocVm>();

        // Breakdowns
        public IReadOnlyList<DocStatusBreakdownVm> ByStatus { get; set; } = new List<DocStatusBreakdownVm>();
        public IReadOnlyList<DocTypeBreakdownVm> ByType { get; set; } = new List<DocTypeBreakdownVm>();
        public IReadOnlyList<DocCategoryBreakdownVm> ByCategory { get; set; } = new List<DocCategoryBreakdownVm>();
        public IReadOnlyList<RecentDocActivityVm> RecentActivity { get; set; } = new List<RecentDocActivityVm>();
    }

    public class ReviewAlertVm
    {
        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTimeOffset ReviewDate { get; set; }
        public int DaysOverdueOrRemaining { get; set; }
    }

    public class PendingApprovalVm
    {
        public int RevisionId { get; set; }
        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string RevisionNumber { get; set; } = null!;
        public DateTimeOffset SubmittedDate { get; set; }
        public int DaysPending { get; set; }
    }

    public class CheckedOutDocVm
    {
        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTimeOffset CheckedOutDate { get; set; }
        public int DaysCheckedOut { get; set; }
    }

    public class DocStatusBreakdownVm
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DocTypeBreakdownVm
    {
        public string Type { get; set; } = null!;
        public int Count { get; set; }
        public int PublishedCount { get; set; }
    }

    public class DocCategoryBreakdownVm
    {
        public string Category { get; set; } = null!;
        public int Count { get; set; }
    }

    public class RecentDocActivityVm
    {
        public string Icon { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public int? DocumentId { get; set; }
    }
}
