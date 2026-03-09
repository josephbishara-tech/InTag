using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Maintenance
{
    public class PMScheduleCreateVm
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "Trigger")]
        public PMTriggerType TriggerType { get; set; }

        [Required]
        public PMScheduleFrequency Frequency { get; set; } = PMScheduleFrequency.Monthly;

        [MaxLength(100)]
        [Display(Name = "Meter Type")]
        public string? MeterType { get; set; }

        [Display(Name = "Meter Interval")]
        public decimal? MeterIntervalValue { get; set; }

        public ConditionRating? TriggerConditionThreshold { get; set; }

        public WorkOrderPriority DefaultPriority { get; set; } = WorkOrderPriority.Medium;

        [Display(Name = "Est. Labor Hours")]
        public decimal? EstimatedLaborHours { get; set; }

        [Display(Name = "SLA Hours")]
        public decimal? SLATargetHours { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Task Description")]
        public string? TaskDescription { get; set; }
    }

    public class PMScheduleDetailVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public PMTriggerType TriggerType { get; set; }
        public string TriggerDisplay => TriggerType.ToString();
        public PMScheduleFrequency Frequency { get; set; }
        public string FrequencyDisplay => Frequency.ToString();
        public DateTimeOffset? LastExecutedDate { get; set; }
        public DateTimeOffset? NextDueDate { get; set; }
        public bool IsOverdue => NextDueDate.HasValue && NextDueDate < DateTimeOffset.UtcNow;
        public string? MeterType { get; set; }
        public decimal? MeterIntervalValue { get; set; }
        public decimal? LastMeterReading { get; set; }
        public decimal? NextMeterThreshold { get; set; }
        public ConditionRating? TriggerConditionThreshold { get; set; }
        public WorkOrderPriority DefaultPriority { get; set; }
        public decimal? EstimatedLaborHours { get; set; }
        public decimal? SLATargetHours { get; set; }
        public bool IsEnabled { get; set; }
        public string? TaskDescription { get; set; }
        public int GeneratedWorkOrderCount { get; set; }
    }

    public class PMScheduleListItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string TriggerDisplay { get; set; } = null!;
        public string FrequencyDisplay { get; set; } = null!;
        public DateTimeOffset? NextDueDate { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsOverdue => NextDueDate.HasValue && NextDueDate < DateTimeOffset.UtcNow && IsEnabled;
    }

    public class PMGenerationResultVm
    {
        public int WorkOrdersGenerated { get; set; }
        public int SchedulesProcessed { get; set; }
        public int SchedulesSkipped { get; set; }
        public IReadOnlyList<PMGenerationItemVm> Details { get; set; } = new List<PMGenerationItemVm>();
    }

    public class PMGenerationItemVm
    {
        public string ScheduleName { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? WorkOrderNumber { get; set; }
        public string? Message { get; set; }
    }
}
