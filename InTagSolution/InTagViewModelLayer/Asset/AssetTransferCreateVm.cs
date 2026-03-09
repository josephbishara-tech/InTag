using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Asset
{
    public class AssetTransferCreateVm
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "From Location")]
        public int FromLocationId { get; set; }

        [Required]
        [Display(Name = "To Location")]
        public int ToLocationId { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class AssetTransferResultVm
    {
        public int TransferId { get; set; }
        public string AssetName { get; set; } = null!;
        public string FromLocationName { get; set; } = null!;
        public string ToLocationName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
