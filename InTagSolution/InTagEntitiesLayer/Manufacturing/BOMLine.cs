using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    public class BOMLine : BaseEntity
    {
        [Required]
        public int BOMId { get; set; }

        [Required]
        public int ComponentProductId { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public UnitOfMeasure UOM { get; set; }

        /// <summary>
        /// Expected scrap percentage (0-100)
        /// </summary>
        public decimal ScrapFactor { get; set; }

        /// <summary>
        /// Phantom assembly — exploded into parent BOM during planning
        /// </summary>
        public bool IsPhantom { get; set; }

        public int SortOrder { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public BillOfMaterial BOM { get; set; } = null!;
        public Product ComponentProduct { get; set; } = null!;
    }
}
