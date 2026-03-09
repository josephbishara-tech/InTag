using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Manufacturing
{
    /// <summary>
    /// Master product/item record — can be raw material, semi-finished, or finished good.
    /// </summary>
    public class Product : BaseEntity
    {
        [Required, MaxLength(50)]
        public string ProductCode { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public UnitOfMeasure UOM { get; set; } = UnitOfMeasure.Each;

        [MaxLength(100)]
        public string? Category { get; set; }

        public bool IsRawMaterial { get; set; }

        public bool IsFinishedGood { get; set; }

        public decimal StandardCost { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public ICollection<BillOfMaterial> BOMs { get; set; } = new List<BillOfMaterial>();
    }
}
