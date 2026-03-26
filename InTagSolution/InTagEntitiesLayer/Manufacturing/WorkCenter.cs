using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    public class WorkCenter : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public WorkCenterStatus Status { get; set; } = WorkCenterStatus.Active;

        /// <summary>
        /// Available hours per day
        /// </summary>
        public decimal CapacityHoursPerDay { get; set; } = 8;

        /// <summary>
        /// Number of parallel stations
        /// </summary>
        public int NumberOfStations { get; set; } = 1;

        /// <summary>
        /// Cost per hour for this work center
        /// </summary>
        public decimal CostPerHour { get; set; }

        /// <summary>
        /// Units this work center can produce per hour
        /// </summary>
        public decimal CapacityPerHour { get; set; }

        /// <summary>
        /// Labor cost rate per hour
        /// </summary>
        public decimal LaborRatePerHour { get; set; }

        /// <summary>
        /// Overhead cost rate per hour
        /// </summary>
        public decimal OverheadRatePerHour { get; set; }

        /// <summary>
        /// Operating efficiency percentage (0-100)
        /// </summary>
        public decimal EfficiencyPercent { get; set; } = 100;

        public int? LocationId { get; set; }

        public int? DepartmentId { get; set; }

        // Navigation
        public Asset.Location? Location { get; set; }
        public Asset.Department? Department { get; set; }
        public ICollection<RoutingOperation> Operations { get; set; } = new List<RoutingOperation>();
    }
}
