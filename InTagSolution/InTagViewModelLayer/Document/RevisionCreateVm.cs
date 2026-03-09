using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Document
{
    public class RevisionCreateVm
    {
        [Required]
        public int DocumentId { get; set; }

        [Required, MaxLength(1000)]
        [Display(Name = "Change Description")]
        public string ChangeDescription { get; set; } = null!;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class RevisionApprovalVm
    {
        [Required]
        public int RevisionId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }

    public class DistributionCreateVm
    {
        [Required]
        public int DocumentId { get; set; }

        [Required, MaxLength(50)]
        public string RecipientType { get; set; } = null!;

        [Required, MaxLength(200)]
        public string RecipientIdentifier { get; set; } = null!;

        [MaxLength(200)]
        public string? RecipientName { get; set; }

        public InTagEntitiesLayer.Enums.DistributionMethod Method { get; set; }
    }
}
