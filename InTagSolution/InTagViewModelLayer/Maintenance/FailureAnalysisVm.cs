using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Maintenance
{
    public class FailureAnalysisVm
    {
        public int TotalFailures { get; set; }
        public decimal TotalDowntimeHours { get; set; }
        public decimal AvgRepairTimeHours { get; set; }
        public IReadOnlyList<FailureByTypeVm> ByType { get; set; } = new List<FailureByTypeVm>();
        public IReadOnlyList<FailureByAssetVm> ByAsset { get; set; } = new List<FailureByAssetVm>();
        public IReadOnlyList<FailureTrendVm> MonthlyTrend { get; set; } = new List<FailureTrendVm>();
        public IReadOnlyList<TopFailureVm> TopRepeatFailures { get; set; } = new List<TopFailureVm>();
    }

    public class FailureByTypeVm
    {
        public string Type { get; set; } = null!;
        public int Count { get; set; }
        public decimal TotalDowntime { get; set; }
        public decimal Percentage { get; set; }
    }

    public class FailureByAssetVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public int FailureCount { get; set; }
        public decimal TotalDowntime { get; set; }
        public decimal AvgRepairTime { get; set; }
    }

    public class FailureTrendVm
    {
        public string Period { get; set; } = null!;
        public int Count { get; set; }
        public decimal DowntimeHours { get; set; }
    }

    public class TopFailureVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public FailureType FailureType { get; set; }
        public int OccurrenceCount { get; set; }
        public string? CommonCause { get; set; }
    }
}
