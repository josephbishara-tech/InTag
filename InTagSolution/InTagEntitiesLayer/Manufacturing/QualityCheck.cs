using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    public class QualityCheck : BaseEntity
    {
        public int? ProductionOrderId { get; set; }

        public int? LotBatchId { get; set; }

        public int? RoutingOperationId { get; set; }

        [Required, MaxLength(200)]
        public string CheckName { get; set; } = null!;

        [MaxLength(500)]
        public string? Specification { get; set; }

        [MaxLength(200)]
        public string? ActualValue { get; set; }

        [Required]
        public QualityCheckResult Result { get; set; } = QualityCheckResult.Pending;

        public Guid? InspectorUserId { get; set; }

        [Required]
        public DateTimeOffset CheckDate { get; set; }

        [MaxLength(1000)]
        public string? Findings { get; set; }

        [MaxLength(1000)]
        public string? CorrectiveAction { get; set; }

        // Navigation
        public ProductionOrder? ProductionOrder { get; set; }
        public LotBatch? LotBatch { get; set; }
        public RoutingOperation? RoutingOperation { get; set; }
    }
}
