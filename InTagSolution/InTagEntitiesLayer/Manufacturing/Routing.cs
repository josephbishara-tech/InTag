using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Manufacturing
{
    public class Routing : BaseEntity
    {
        [Required, MaxLength(50)]
        public string RoutingCode { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [Required, MaxLength(20)]
        public string Version { get; set; } = "1.0";

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sum of all operation cycle times (minutes)
        /// </summary>
        public decimal TotalCycleTimeMinutes { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public ICollection<RoutingOperation> Operations { get; set; } = new List<RoutingOperation>();
    }
}
