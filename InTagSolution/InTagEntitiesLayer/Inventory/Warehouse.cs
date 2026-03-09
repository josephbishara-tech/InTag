using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Inventory
{
    public class Warehouse : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public int? LocationId { get; set; }

        // Navigation
        public Asset.Location? Location { get; set; }
        public ICollection<StorageBin> Bins { get; set; } = new List<StorageBin>();
    }
}
