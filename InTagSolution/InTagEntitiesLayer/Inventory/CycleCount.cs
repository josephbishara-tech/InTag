using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Inventory
{
    public class CycleCount : BaseEntity
    {
        [Required, MaxLength(50)]
        public string CountNumber { get; set; } = null!;

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public DateTimeOffset CountDate { get; set; }

        public Guid? CountedByUserId { get; set; }

        public bool IsCompleted { get; set; }

        public DateTimeOffset? CompletedDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public Warehouse Warehouse { get; set; } = null!;
        public ICollection<CycleCountLine> Lines { get; set; } = new List<CycleCountLine>();
    }
}
