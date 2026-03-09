using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    public class AssetType : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DepreciationMethod DefaultDepreciationMethod { get; set; }

        /// <summary>
        /// Default useful life in months
        /// </summary>
        [Required]
        public int UsefulLifeMonths { get; set; }

        [Required]
        public AssetCategory Category { get; set; }

        /// <summary>
        /// Default salvage value percentage (e.g. 10 = 10%)
        /// </summary>
        public decimal DefaultSalvageValuePercent { get; set; }

        // Navigation
        public ICollection<AssetItem> Assets { get; set; } = new List<AssetItem>();
    }
}
