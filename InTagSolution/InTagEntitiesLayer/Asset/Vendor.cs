using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Asset
{
    public class Vendor : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        public string? Code { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public ICollection<AssetItem> Assets { get; set; } = new List<AssetItem>();
    }
}
