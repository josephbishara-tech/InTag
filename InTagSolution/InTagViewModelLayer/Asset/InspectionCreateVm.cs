using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class InspectionCreateVm
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "Condition Score")]
        public ConditionRating ConditionScore { get; set; }

        [Required, MaxLength(2000)]
        public string Findings { get; set; } = null!;

        [MaxLength(2000)]
        public string? Recommendations { get; set; }

        [Display(Name = "Next Due Date")]
        public DateTimeOffset? NextDueDate { get; set; }

        [MaxLength(200)]
        [Display(Name = "Checklist")]
        public string? ChecklistName { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class InspectionResultVm
    {
        public int InspectionId { get; set; }
        public string AssetName { get; set; } = null!;
        public string ConditionScore { get; set; } = null!;
        public DateTimeOffset InspectionDate { get; set; }
        public DateTimeOffset? NextDueDate { get; set; }
    }
}
