using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Inventory
{
    public class CycleCountLine : BaseEntity
    {
        [Required]
        public int CycleCountId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? StorageBinId { get; set; }

        public decimal SystemQuantity { get; set; }

        public decimal CountedQuantity { get; set; }

        public decimal Variance => CountedQuantity - SystemQuantity;

        public decimal VariancePercent => SystemQuantity != 0
            ? Math.Round(Variance / SystemQuantity * 100, 2) : 0;

        public bool IsAdjusted { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public CycleCount CycleCount { get; set; } = null!;
        public Manufacturing.Product Product { get; set; } = null!;
        public StorageBin? StorageBin { get; set; }
    }
}
