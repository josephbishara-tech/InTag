using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Maintenance
{
    public class WorkOrderCreateVm
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public WorkOrderType Type { get; set; }

        [Required]
        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

        [Required]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [Display(Name = "Due Date")]
        public DateTimeOffset? DueDate { get; set; }

        [Display(Name = "SLA Hours")]
        public decimal? SLATargetHours { get; set; }

        public FailureType? FailureType { get; set; }

        [MaxLength(1000)]
        public string? FailureCause { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class WorkOrderDetailVm
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public WorkOrderType Type { get; set; }
        public string TypeDisplay => Type.ToString();
        public WorkOrderPriority Priority { get; set; }
        public string PriorityDisplay => Priority.ToString();
        public WorkOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string? PMScheduleName { get; set; }
        public FailureType? FailureType { get; set; }
        public string? FailureCause { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartedDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public decimal? SLATargetHours { get; set; }
        public decimal? ActualHours { get; set; }
        public bool IsSLABreached => SLATargetHours.HasValue && ActualHours.HasValue && ActualHours > SLATargetHours;
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTimeOffset.UtcNow
                                 && Status != WorkOrderStatus.Completed && Status != WorkOrderStatus.Closed;
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal ExternalCost { get; set; }
        public decimal TotalCost => LaborCost + PartsCost + ExternalCost;
        public string? Resolution { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public IReadOnlyList<LaborEntryVm> LaborEntries { get; set; } = new List<LaborEntryVm>();
        public IReadOnlyList<PartEntryVm> Parts { get; set; } = new List<PartEntryVm>();
    }

    public class WorkOrderListItemVm
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public WorkOrderType Type { get; set; }
        public string TypeDisplay => Type.ToString();
        public WorkOrderPriority Priority { get; set; }
        public string PriorityDisplay => Priority.ToString();
        public WorkOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTimeOffset.UtcNow
                                 && Status != WorkOrderStatus.Completed && Status != WorkOrderStatus.Closed;
    }

    public class WorkOrderFilterVm
    {
        public string? SearchTerm { get; set; }
        public WorkOrderStatus? Status { get; set; }
        public WorkOrderType? Type { get; set; }
        public WorkOrderPriority? Priority { get; set; }
        public int? AssetId { get; set; }
        public string? SortBy { get; set; } = "WorkOrderNumber";
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkOrderListResultVm
    {
        public IReadOnlyList<WorkOrderListItemVm> Items { get; set; } = new List<WorkOrderListItemVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class LaborEntryVm
    {
        public int Id { get; set; }
        public string? TechnicianName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Cost { get; set; }
        public string? WorkPerformed { get; set; }
    }

    public class LaborEntryCreateVm
    {
        [Required]
        public int WorkOrderId { get; set; }

        [MaxLength(200)]
        [Display(Name = "Technician")]
        public string? TechnicianName { get; set; }

        [Required]
        [Range(0.01, 1000)]
        [Display(Name = "Hours")]
        public decimal HoursWorked { get; set; }

        [Display(Name = "Rate/Hr")]
        public decimal HourlyRate { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Work Performed")]
        public string? WorkPerformed { get; set; }
    }

    public class PartEntryVm
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class PartEntryCreateVm
    {
        [Required]
        public int WorkOrderId { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Part #")]
        public string PartNumber { get; set; } = null!;

        [Required, MaxLength(300)]
        [Display(Name = "Part Name")]
        public string PartName { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }
    }

    public class WorkOrderCompleteVm
    {
        [Required]
        public int WorkOrderId { get; set; }

        [MaxLength(2000)]
        public string? Resolution { get; set; }

        public decimal ExternalCost { get; set; }
    }
}
