using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    public class AssetTransfer : BaseEntity
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        public int FromLocationId { get; set; }

        [Required]
        public int ToLocationId { get; set; }

        [Required]
        public DateTimeOffset TransferDate { get; set; }

        [Required]
        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;

        public Guid? ApprovedByUserId { get; set; }

        public DateTimeOffset? ApprovedDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public AssetItem Asset { get; set; } = null!;
        public Location FromLocation { get; set; } = null!;
        public Location ToLocation { get; set; } = null!;
    }
}
