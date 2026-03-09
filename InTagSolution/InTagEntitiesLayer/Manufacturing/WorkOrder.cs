using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Maintenance
{
    public class WorkOrder : BaseEntity
    {
        [Required, MaxLength(50)]
        public string WorkOrderNumber { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public WorkOrderType Type { get; set; }

        [Required]
        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

        [Required]
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;

        // ── Asset Link ───────────────────────
        [Required]
        public int AssetId { get; set; }

        // ── Assignment ───────────────────────
        public Guid? AssignedToUserId { get; set; }

        public int? PMScheduleId { get; set; }

        // ── Failure ──────────────────────────
        public FailureType? FailureType { get; set; }

        [MaxLength(1000)]
        public string? FailureCause { get; set; }

        // ── Dates / SLA ──────────────────────
        public DateTimeOffset? DueDate { get; set; }

        public DateTimeOffset? StartedDate { get; set; }

        public DateTimeOffset? CompletedDate { get; set; }

        /// <summary>
        /// SLA target hours from open to completion
        /// </summary>
        public decimal? SLATargetHours { get; set; }

        // ── Costs ────────────────────────────
        public decimal LaborCost { get; set; }

        public decimal PartsCost { get; set; }

        public decimal ExternalCost { get; set; }

        public decimal TotalCost => LaborCost + PartsCost + ExternalCost;

        // ── Resolution ───────────────────────
        [MaxLength(2000)]
        public string? Resolution { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public Asset.AssetItem Asset { get; set; } = null!;
        public PMSchedule? PMSchedule { get; set; }
        public ICollection<WorkOrderLabor> LaborEntries { get; set; } = new List<WorkOrderLabor>();
        public ICollection<WorkOrderPart> Parts { get; set; } = new List<WorkOrderPart>();
    }
}
