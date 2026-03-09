using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class AssetDashboardVm
    {
        // KPIs
        public int TotalAssets { get; set; }
        public int OperationalAssets { get; set; }
        public int UnderMaintenanceAssets { get; set; }
        public int DecommissionedAssets { get; set; }
        public decimal TotalAssetValue { get; set; }
        public decimal TotalBookValue { get; set; }

        // Alerts
        public IReadOnlyList<WarrantyAlertVm> WarrantyAlerts { get; set; } = new List<WarrantyAlertVm>();
        public IReadOnlyList<InspectionAlertVm> InspectionAlerts { get; set; } = new List<InspectionAlertVm>();
        public IReadOnlyList<ConditionAlertVm> ConditionAlerts { get; set; } = new List<ConditionAlertVm>();

        // Breakdowns
        public IReadOnlyList<StatusBreakdownVm> ByStatus { get; set; } = new List<StatusBreakdownVm>();
        public IReadOnlyList<CategoryBreakdownVm> ByCategory { get; set; } = new List<CategoryBreakdownVm>();
        public IReadOnlyList<LocationBreakdownVm> ByLocation { get; set; } = new List<LocationBreakdownVm>();
        public IReadOnlyList<RecentActivityVm> RecentActivity { get; set; } = new List<RecentActivityVm>();
    }

    public class WarrantyAlertVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTimeOffset WarrantyEndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class InspectionAlertVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTimeOffset DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class ConditionAlertVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public ConditionRating ConditionScore { get; set; }
        public DateTimeOffset InspectionDate { get; set; }
    }

    public class StatusBreakdownVm
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class CategoryBreakdownVm
    {
        public string Category { get; set; } = null!;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class LocationBreakdownVm
    {
        public string Location { get; set; } = null!;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class RecentActivityVm
    {
        public string Icon { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public int? AssetId { get; set; }
    }
}
