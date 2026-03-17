using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Asset
{
    public class Location : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        public string? Code { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Building { get; set; }

        [MaxLength(50)]
        public string? Floor { get; set; }

        [MaxLength(50)]
        public string? Room { get; set; }

        public int? ParentLocationId { get; set; }


        // Navigation
        public Location? ParentLocation { get; set; }
        public ICollection<Location> ChildLocations { get; set; } = new List<Location>();
        public ICollection<AssetItem> Assets { get; set; } = new List<AssetItem>();
    }
}
