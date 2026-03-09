using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class ManufacturingDashboardVm
    {
        // KPIs
        public int TotalOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int ReleasedOrders { get; set; }
        public int CompletedOrdersThisMonth { get; set; }
        public decimal TotalPlannedQty { get; set; }
        public decimal TotalCompletedQty { get; set; }
        public decimal OverallCompletionPercent { get; set; }
        public decimal AverageOEE { get; set; }

        // Quality
        public int TotalQualityChecks { get; set; }
        public int QualityPassCount { get; set; }
        public int QualityFailCount { get; set; }
        public decimal QualityPassRate { get; set; }
        public decimal ScrapRate { get; set; }

        // Alerts
        public IReadOnlyList<OverdueOrderVm> OverdueOrders { get; set; } = new List<OverdueOrderVm>();
        public IReadOnlyList<QuarantinedLotVm> QuarantinedLots { get; set; } = new List<QuarantinedLotVm>();
        public IReadOnlyList<UrgentOrderVm> UrgentOrders { get; set; } = new List<UrgentOrderVm>();

        // Breakdowns
        public IReadOnlyList<OrderStatusBreakdownVm> ByStatus { get; set; } = new List<OrderStatusBreakdownVm>();
        public IReadOnlyList<ProductionByProductVm> ByProduct { get; set; } = new List<ProductionByProductVm>();
        public IReadOnlyList<WorkCenterCapacityVm> WorkCenterLoad { get; set; } = new List<WorkCenterCapacityVm>();
        public IReadOnlyList<RecentProductionActivityVm> RecentActivity { get; set; } = new List<RecentProductionActivityVm>();
    }

    public class OverdueOrderVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public DateTimeOffset PlannedEndDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class QuarantinedLotVm
    {
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public DateTimeOffset ManufactureDate { get; set; }
    }

    public class UrgentOrderVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal CompletionPercent { get; set; }
        public DateTimeOffset? PlannedEndDate { get; set; }
    }

    public class OrderStatusBreakdownVm
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ProductionByProductVm
    {
        public string ProductName { get; set; } = null!;
        public int OrderCount { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal TotalCompleted { get; set; }
        public decimal TotalScrap { get; set; }
    }

    public class RecentProductionActivityVm
    {
        public string Icon { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public int? OrderId { get; set; }
    }
}
