using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    public class TrackingRequest : BaseEntity
    {
        [Required, MaxLength(50)]
        public string RequestNumber { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TrackingRequestStatus Status { get; set; } = TrackingRequestStatus.Draft;

        [Required]
        public int LocationId { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public DateTimeOffset? StartedDate { get; set; }

        public DateTimeOffset? CompletedDate { get; set; }

        public int TotalAssets { get; set; }

        public int FoundCount { get; set; }

        public int MissingCount { get; set; }

        public int RelocatedCount { get; set; }

        public int DamagedCount { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public Location Location { get; set; } = null!;
        public ICollection<TrackingLine> Lines { get; set; } = new List<TrackingLine>();
    }
}
