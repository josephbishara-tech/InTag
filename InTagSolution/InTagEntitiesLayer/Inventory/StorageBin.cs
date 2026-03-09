using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Inventory
{
    public class StorageBin : BaseEntity
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required, MaxLength(50)]
        public string BinCode { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Aisle { get; set; }

        [MaxLength(50)]
        public string? Shelf { get; set; }

        [MaxLength(50)]
        public string? Level { get; set; }

        public decimal? MaxCapacity { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public Warehouse Warehouse { get; set; } = null!;
    }
}
