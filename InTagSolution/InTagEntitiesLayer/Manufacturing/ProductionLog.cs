using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Manufacturing
{
    /// <summary>
    /// Shop floor reporting — records actual production output per shift/operation
    /// </summary>
    public class ProductionLog : BaseEntity
    {
        [Required]
        public int ProductionOrderId { get; set; }

        public int? RoutingOperationId { get; set; }

        public int? WorkCenterId { get; set; }

        public Guid? OperatorUserId { get; set; }

        [Required]
        public decimal QuantityProduced { get; set; }

        public decimal QuantityScrapped { get; set; }

        public decimal QuantityRework { get; set; }

        [Required]
        public DateTimeOffset LogDate { get; set; }

        public decimal? SetupTimeActual { get; set; }

        public decimal? RunTimeActual { get; set; }

        public decimal? DowntimeMinutes { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public ProductionOrder ProductionOrder { get; set; } = null!;
        public RoutingOperation? RoutingOperation { get; set; }
        public WorkCenter? WorkCenter { get; set; }
    }
}
