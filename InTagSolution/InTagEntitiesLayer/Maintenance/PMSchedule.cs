using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Maintenance
{
    /// <summary>
    /// Preventive Maintenance schedule — generates work orders automatically.
    /// </summary>
    public class PMSchedule : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public PMTriggerType TriggerType { get; set; }

        // ── Calendar-Based ───────────────────
        [Required]
        public PMScheduleFrequency Frequency { get; set; } = PMScheduleFrequency.Monthly;

        public DateTimeOffset? LastExecutedDate { get; set; }

        public DateTimeOffset? NextDueDate { get; set; }

        // ── Meter-Based ──────────────────────
        [MaxLength(100)]
        public string? MeterType { get; set; }

        public decimal? MeterIntervalValue { get; set; }

        public decimal? LastMeterReading { get; set; }

        public decimal? NextMeterThreshold { get; set; }

        // ── Condition-Based ──────────────────
        public ConditionRating? TriggerConditionThreshold { get; set; }

        // ── Assignment defaults ──────────────
        public Guid? DefaultAssigneeUserId { get; set; }

        public WorkOrderPriority DefaultPriority { get; set; } = WorkOrderPriority.Medium;

        public decimal? EstimatedLaborHours { get; set; }

        public decimal? SLATargetHours { get; set; }

        public bool IsEnabled { get; set; } = true;

        [MaxLength(2000)]
        public string? TaskDescription { get; set; }

        // Navigation
        public Asset.AssetItem Asset { get; set; } = null!;
        public ICollection<WorkOrder> GeneratedWorkOrders { get; set; } = new List<WorkOrder>();
    }
}
