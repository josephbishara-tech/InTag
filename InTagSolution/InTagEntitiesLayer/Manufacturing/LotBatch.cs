using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    public class LotBatch : BaseEntity
    {
        [Required, MaxLength(50)]
        public string LotNumber { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        public int? ProductionOrderId { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public LotBatchStatus Status { get; set; } = LotBatchStatus.Created;

        [Required]
        public DateTimeOffset ManufactureDate { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        [MaxLength(200)]
        public string? StorageLocation { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public ProductionOrder? ProductionOrder { get; set; }
        public ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
    }
}
