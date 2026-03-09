using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Manufacturing
{
    public class RoutingOperation : BaseEntity
    {
        [Required]
        public int RoutingId { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required, MaxLength(200)]
        public string OperationName { get; set; } = null!;

        [Required]
        public int WorkCenterId { get; set; }

        /// <summary>
        /// One-time setup time in minutes
        /// </summary>
        public decimal SetupTimeMinutes { get; set; }

        /// <summary>
        /// Run time per unit in minutes
        /// </summary>
        public decimal RunTimePerUnitMinutes { get; set; }

        /// <summary>
        /// Number of units that can overlap with next operation
        /// </summary>
        public int OverlapQuantity { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        // Navigation
        public Routing Routing { get; set; } = null!;
        public WorkCenter WorkCenter { get; set; } = null!;
    }
}
