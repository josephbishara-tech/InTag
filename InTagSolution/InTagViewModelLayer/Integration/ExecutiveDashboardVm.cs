namespace InTagViewModelLayer.Integration
{
    public class ExecutiveDashboardVm
    {
        // Asset KPIs
        public int TotalAssets { get; set; }
        public decimal TotalAssetValue { get; set; }
        public decimal AssetDepreciationThisYear { get; set; }
        public int AssetsRequiringAttention { get; set; }

        // Document KPIs
        public int TotalDocuments { get; set; }
        public int DocumentsPublished { get; set; }
        public int DocumentsOverdueReview { get; set; }
        public decimal DocumentCompliancePercent { get; set; }

        // Manufacturing KPIs
        public int ProductionOrdersInProgress { get; set; }
        public int ProductionOrdersCompleted { get; set; }
        public decimal OverallOEE { get; set; }
        public decimal QualityPassRate { get; set; }

        // Maintenance KPIs
        public int OpenWorkOrders { get; set; }
        public int OverdueWorkOrders { get; set; }
        public decimal PMCompliancePercent { get; set; }
        public decimal SLACompliancePercent { get; set; }

        // Inventory KPIs
        public decimal InventoryValue { get; set; }
        public int OutOfStockItems { get; set; }
        public decimal InventoryTurnover { get; set; }
        public int ExpiringItems { get; set; }

        // Workflow KPIs
        public int PendingApprovals { get; set; }
        public int CompletedWorkflows { get; set; }
        public int EscalatedWorkflows { get; set; }

        // Cross-module
        public IReadOnlyList<ModuleSummaryVm> ModuleSummaries { get; set; } = new List<ModuleSummaryVm>();
        public IReadOnlyList<CriticalAlertVm> CriticalAlerts { get; set; } = new List<CriticalAlertVm>();
        public IReadOnlyList<RecentActivityVm> RecentActivity { get; set; } = new List<RecentActivityVm>();
        public IReadOnlyList<CostSummaryVm> MonthlyCostTrend { get; set; } = new List<CostSummaryVm>();
    }

    public class ModuleSummaryVm
    {
        public string Module { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string PrimaryMetric { get; set; } = null!;
        public string PrimaryLabel { get; set; } = null!;
        public string SecondaryMetric { get; set; } = null!;
        public string SecondaryLabel { get; set; } = null!;
        public string Url { get; set; } = null!;
    }

    public class CriticalAlertVm
    {
        public string Module { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ActionUrl { get; set; }
        public DateTimeOffset Date { get; set; }
    }

    public class RecentActivityVm
    {
        public string Module { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public string? Url { get; set; }
    }

    public class CostSummaryVm
    {
        public string Period { get; set; } = null!;
        public decimal MaintenanceCost { get; set; }
        public decimal DepreciationCost { get; set; }
        public decimal InventoryValue { get; set; }
    }
}
