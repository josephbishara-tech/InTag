using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    public class DocumentUpdateVm
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DocumentCategory Category { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        public ReviewCycle ReviewCycle { get; set; }

        [MaxLength(200)]
        public string? IsoReference { get; set; }

        [MaxLength(100)]
        public string? ConfidentialityLevel { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
