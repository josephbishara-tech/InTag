using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Maintenance
{
    public class SLAReportVm
    {
        public int TotalWithSLA { get; set; }
        public int SLAMetCount { get; set; }
        public int SLABreachedCount { get; set; }
        public decimal SLACompliancePercent { get; set; }
        public decimal AvgResponseHours { get; set; }
        public decimal AvgResolutionHours { get; set; }
        public IReadOnlyList<SLAWorkOrderVm> BreachedOrders { get; set; } = new List<SLAWorkOrderVm>();
        public IReadOnlyList<SLAByPriorityVm> ByPriority { get; set; } = new List<SLAByPriorityVm>();
        public IReadOnlyList<BacklogItemVm> Backlog { get; set; } = new List<BacklogItemVm>();
    }

    public class SLAWorkOrderVm
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public WorkOrderPriority Priority { get; set; }
        public decimal SLATargetHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal OverageHours { get; set; }
    }

    public class SLAByPriorityVm
    {
        public string Priority { get; set; } = null!;
        public int Total { get; set; }
        public int Met { get; set; }
        public int Breached { get; set; }
        public decimal CompliancePercent { get; set; }
        public decimal AvgResolutionHours { get; set; }
    }

    public class BacklogItemVm
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public WorkOrderPriority Priority { get; set; }
        public string PriorityDisplay => Priority.ToString();
        public int AgeDays { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class MaintenanceCostByAssetVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public int WorkOrderCount { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal ExternalCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
