namespace InTagViewModelLayer.Integration
{
    public class ComplianceReportVm
    {
        public decimal OverallComplianceScore { get; set; }
        public IReadOnlyList<ComplianceModuleVm> Modules { get; set; } = new List<ComplianceModuleVm>();
        public IReadOnlyList<ComplianceIssueVm> Issues { get; set; } = new List<ComplianceIssueVm>();
    }

    public class ComplianceModuleVm
    {
        public string Module { get; set; } = null!;
        public decimal Score { get; set; }
        public int TotalChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
    }

    public class ComplianceIssueVm
    {
        public string Module { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ActionUrl { get; set; }
    }

    public class AuditTrailFilterVm
    {
        public string? Module { get; set; }
        public string? EntityType { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class AuditTrailResultVm
    {
        public IReadOnlyList<AuditEntryVm> Items { get; set; } = new List<AuditEntryVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class AuditEntryVm
    {
        public string Module { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? EntityReference { get; set; }
        public string? UserName { get; set; }
        public DateTimeOffset Date { get; set; }
        public string? Details { get; set; }
    }
}
