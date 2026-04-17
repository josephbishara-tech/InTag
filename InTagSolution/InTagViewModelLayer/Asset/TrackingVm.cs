using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class TrackingRequestCreateVm
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Location")]
        public int LocationId { get; set; }

        public Guid? AssignedToUserId { get; set; }
    }

    public class TrackingRequestDetailVm
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TrackingRequestStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public int LocationId { get; set; }
        public string LocationName { get; set; } = null!;
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? StartedDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public int TotalAssets { get; set; }
        public int FoundCount { get; set; }
        public int MissingCount { get; set; }
        public int RelocatedCount { get; set; }
        public int DamagedCount { get; set; }
        public int PendingCount => TotalAssets - FoundCount - MissingCount - RelocatedCount - DamagedCount;
        public decimal ProgressPercent => TotalAssets > 0
            ? Math.Round((TotalAssets - PendingCount) * 100m / TotalAssets, 1) : 0;
        public string? Notes { get; set; }
        public IReadOnlyList<TrackingLineVm> Lines { get; set; } = new List<TrackingLineVm>();
    }

    public class TrackingRequestListVm
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string LocationName { get; set; } = null!;
        public TrackingRequestStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public int TotalAssets { get; set; }
        public int ScannedCount { get; set; }
        public decimal ProgressPercent => TotalAssets > 0
            ? Math.Round(ScannedCount * 100m / TotalAssets, 1) : 0;
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class TrackingLineVm
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string? Barcode { get; set; }
        public string? SerialNumber { get; set; }
        public TrackingLineStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset? ScannedDate { get; set; }
        public string? ScannedCode { get; set; }
        public string? FoundAtLocationName { get; set; }
        public ConditionRating ConditionAtScan { get; set; }
        public string? Remarks { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// Used by MAUI app to submit scan results via API
    /// </summary>
    public class TrackingScanSubmitVm
    {
        [Required]
        public int TrackingLineId { get; set; }

        [Required]
        public TrackingLineStatus Status { get; set; }

        [MaxLength(200)]
        public string? ScannedCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public int? FoundAtLocationId { get; set; }

        public ConditionRating ConditionAtScan { get; set; } = ConditionRating.Good;

        [MaxLength(1000)]
        public string? Remarks { get; set; }
    }
}
