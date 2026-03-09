using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Manufacturing;

namespace InTagViewModelLayer.Manufacturing
{
    public class ProductionOrderCreateVm
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        public int? BOMId { get; set; }

        public int? RoutingId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        [Display(Name = "Quantity")]
        public decimal PlannedQuantity { get; set; }

        [Required]
        public ProductionPriority Priority { get; set; } = ProductionPriority.Normal;

        [Display(Name = "Planned Start")]
        public DateTimeOffset? PlannedStartDate { get; set; }

        [Display(Name = "Planned End")]
        public DateTimeOffset? PlannedEndDate { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class ProductionOrderDetailVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string? BOMCode { get; set; }
        public string? RoutingCode { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal CompletedQuantity { get; set; }
        public decimal ScrapQuantity { get; set; }
        public decimal CompletionPercent => PlannedQuantity > 0 ? Math.Round(CompletedQuantity / PlannedQuantity * 100, 1) : 0;
        public ProductionOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public ProductionPriority Priority { get; set; }
        public string PriorityDisplay => Priority.ToString();
        public DateTimeOffset? PlannedStartDate { get; set; }
        public DateTimeOffset? PlannedEndDate { get; set; }
        public DateTimeOffset? ActualStartDate { get; set; }
        public DateTimeOffset? ActualEndDate { get; set; }
        public string? Notes { get; set; }

        // Related
        public IReadOnlyList<BOMLineVm> BOMLines { get; set; } = new List<BOMLineVm>();
        public IReadOnlyList<RoutingOperationVm> Operations { get; set; } = new List<RoutingOperationVm>();
        public IReadOnlyList<ProductionLogVm> Logs { get; set; } = new List<ProductionLogVm>();
        public IReadOnlyList<LotBatchListVm> LotBatches { get; set; } = new List<LotBatchListVm>();
        public IReadOnlyList<QualityCheckVm> QualityChecks { get; set; } = new List<QualityCheckVm>();
        public OEEResultVm? OEE { get; set; }
    }

    public class ProductionOrderListItemVm
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal PlannedQuantity { get; set; }
        public decimal CompletedQuantity { get; set; }
        public ProductionOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public ProductionPriority Priority { get; set; }
        public string PriorityDisplay => Priority.ToString();
        public DateTimeOffset? PlannedStartDate { get; set; }
    }

    public class ProductionOrderFilterVm
    {
        public string? SearchTerm { get; set; }
        public ProductionOrderStatus? Status { get; set; }
        public ProductionPriority? Priority { get; set; }
        public int? ProductId { get; set; }
        public string? SortBy { get; set; } = "OrderNumber";
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ProductionOrderListResultVm
    {
        public IReadOnlyList<ProductionOrderListItemVm> Items { get; set; } = new List<ProductionOrderListItemVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
