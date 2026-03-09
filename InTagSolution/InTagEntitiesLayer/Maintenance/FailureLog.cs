using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Maintenance
{
    /// <summary>
    /// Records each failure event for MTBF/MTTR analysis.
    /// </summary>
    public class FailureLog : BaseEntity
    {
        [Required]
        public int AssetId { get; set; }

        public int? WorkOrderId { get; set; }

        [Required]
        public FailureType FailureType { get; set; }

        [Required]
        public DateTimeOffset FailureDate { get; set; }

        public DateTimeOffset? RepairStartDate { get; set; }

        public DateTimeOffset? RepairEndDate { get; set; }

        /// <summary>
        /// Time to repair in hours
        /// </summary>
        public decimal? DowntimeHours { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? RootCause { get; set; }

        [MaxLength(1000)]
        public string? CorrectiveAction { get; set; }

        // Navigation
        public Asset.AssetItem Asset { get; set; } = null!;
        public WorkOrder? WorkOrder { get; set; }
    }
}
