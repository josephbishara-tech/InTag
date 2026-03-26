using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    public class TrackingLine : BaseEntity
    {
        [Required]
        public int TrackingRequestId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public TrackingLineStatus Status { get; set; } = TrackingLineStatus.Pending;

        public DateTimeOffset? ScannedDate { get; set; }

        public Guid? ScannedByUserId { get; set; }

        /// <summary>
        /// GPS latitude from mobile device
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// GPS longitude from mobile device
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Scanned barcode/QR value for verification
        /// </summary>
        [MaxLength(200)]
        public string? ScannedCode { get; set; }

        /// <summary>
        /// If asset found at a different location
        /// </summary>
        public int? FoundAtLocationId { get; set; }

        [Required]
        public ConditionRating ConditionAtScan { get; set; } = ConditionRating.Good;

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        // Navigation
        public TrackingRequest TrackingRequest { get; set; } = null!;
        public AssetItem Asset { get; set; } = null!;
        public Location? FoundAtLocation { get; set; }
    }
}
