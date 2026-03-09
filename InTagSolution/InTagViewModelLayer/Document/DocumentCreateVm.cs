using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    public class DocumentCreateVm
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public DocumentType Type { get; set; }

        [Required]
        public DocumentCategory Category { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Required]
        [Display(Name = "Review Cycle")]
        public ReviewCycle ReviewCycle { get; set; } = ReviewCycle.Annual;

        [MaxLength(200)]
        [Display(Name = "ISO Reference")]
        public string? IsoReference { get; set; }

        [MaxLength(100)]
        [Display(Name = "Confidentiality")]
        public string? ConfidentialityLevel { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Optional: override auto-generated doc number
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "Document Number")]
        public string? DocNumberOverride { get; set; }
    }
}
