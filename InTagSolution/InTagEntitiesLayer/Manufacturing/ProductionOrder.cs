using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    public class ProductionOrder : BaseEntity
    {
        [Required, MaxLength(50)]
        public string OrderNumber { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        public int? BOMId { get; set; }

        public int? RoutingId { get; set; }

        [Required]
        public decimal PlannedQuantity { get; set; }

        public decimal CompletedQuantity { get; set; }

        public decimal ScrapQuantity { get; set; }

        [Required]
        public ProductionOrderStatus Status { get; set; } = ProductionOrderStatus.Draft;

        [Required]
        public ProductionPriority Priority { get; set; } = ProductionPriority.Normal;

        public DateTimeOffset? PlannedStartDate { get; set; }

        public DateTimeOffset? PlannedEndDate { get; set; }

        public DateTimeOffset? ActualStartDate { get; set; }

        public DateTimeOffset? ActualEndDate { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public BillOfMaterial? BOM { get; set; }
        public Routing? Routing { get; set; }
        public ICollection<ProductionLog> ProductionLogs { get; set; } = new List<ProductionLog>();
        public ICollection<LotBatch> LotBatches { get; set; } = new List<LotBatch>();
        public ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
    }
}
