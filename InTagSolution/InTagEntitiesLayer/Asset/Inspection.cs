using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    public class Inspection : BaseEntity
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        public DateTimeOffset InspectionDate { get; set; }

        [Required]
        public ConditionRating ConditionScore { get; set; }

        [Required, MaxLength(2000)]
        public string Findings { get; set; } = null!;

        [MaxLength(2000)]
        public string? Recommendations { get; set; }

        public DateTimeOffset? NextDueDate { get; set; }

        [Required]
        public Guid InspectorUserId { get; set; }

        [MaxLength(200)]
        public string? ChecklistName { get; set; }

        /// <summary>
        /// JSON-serialized checklist items and results
        /// </summary>
        public string? ChecklistData { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public AssetItem Asset { get; set; } = null!;
    }
}
