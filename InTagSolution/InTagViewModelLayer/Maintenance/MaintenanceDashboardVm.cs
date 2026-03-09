using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Maintenance
{
    public class MaintenanceDashboardVm
    {
        // KPIs
        public int OpenWorkOrders { get; set; }
        public int InProgressWorkOrders { get; set; }
        public int CompletedThisMonth { get; set; }
        public int OverdueWorkOrders { get; set; }
        public int OnHoldWorkOrders { get; set; }
        public decimal TotalMaintenanceCostThisMonth { get; set; }

        // PM Compliance
        public int TotalPMSchedules { get; set; }
        public int PMsDueCount { get; set; }
        public int PMsOverdueCount { get; set; }
        public decimal PMCompliancePercent { get; set; }

        // Reliability
        public decimal AvgMTBF { get; set; }
        public decimal AvgMTTR { get; set; }

        // SLA
        public decimal SLACompliancePercent { get; set; }
        public int SLABreachedCount { get; set; }

        // Alerts
        public IReadOnlyList<OverdueWOAlertVm> OverdueAlerts { get; set; } = new List<OverdueWOAlertVm>();
        public IReadOnlyList<PMDueAlertVm> PMDueAlerts { get; set; } = new List<PMDueAlertVm>();
        public IReadOnlyList<CriticalWOAlertVm> CriticalAlerts { get; set; } = new List<CriticalWOAlertVm>();

        // Breakdowns
        public IReadOnlyList<WOStatusBreakdownVm> ByStatus { get; set; } = new List<WOStatusBreakdownVm>();
        public IReadOnlyList<WOTypeBreakdownVm> ByType { get; set; } = new List<WOTypeBreakdownVm>();
        public IReadOnlyList<TopCostAssetVm> TopCostAssets { get; set; } = new List<TopCostAssetVm>();
        public IReadOnlyList<RecentWOActivityVm> RecentActivity { get; set; } = new List<RecentWOActivityVm>();
    }

    public class OverdueWOAlertVm
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public WorkOrderPriority Priority { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class PMDueAlertVm
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public DateTimeOffset DueDate { get; set; }
        public int DaysOverdueOrRemaining { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class CriticalWOAlertVm
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public int AgeDays { get; set; }
    }

    public class WOStatusBreakdownVm
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class WOTypeBreakdownVm
    {
        public string Type { get; set; } = null!;
        public int Count { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class TopCostAssetVm
    {
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public int WOCount { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class RecentWOActivityVm
    {
        public string Icon { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public int? WorkOrderId { get; set; }
    }
}
