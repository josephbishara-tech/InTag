using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    public class BillOfMaterial : BaseEntity
    {
        [Required, MaxLength(50)]
        public string BOMCode { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [Required, MaxLength(20)]
        public string Version { get; set; } = "1.0";

        [Required]
        public BOMStatus Status { get; set; } = BOMStatus.Draft;

        public DateTimeOffset? EffectiveDate { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        /// <summary>
        /// Quantity this BOM produces
        /// </summary>
        public decimal OutputQuantity { get; set; } = 1;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public ICollection<BOMLine> Lines { get; set; } = new List<BOMLine>();
    }
}
