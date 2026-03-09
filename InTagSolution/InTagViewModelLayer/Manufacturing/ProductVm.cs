using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class ProductCreateVm
    {
        [Required, MaxLength(50)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; } = null!;

        [Required, MaxLength(300)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public UnitOfMeasure UOM { get; set; } = UnitOfMeasure.Each;

        [MaxLength(100)]
        public string? Category { get; set; }

        [Display(Name = "Raw Material")]
        public bool IsRawMaterial { get; set; }

        [Display(Name = "Finished Good")]
        public bool IsFinishedGood { get; set; }

        [Display(Name = "Standard Cost")]
        [Range(0, double.MaxValue)]
        public decimal StandardCost { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class ProductListItemVm
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string UOM { get; set; } = null!;
        public string? Category { get; set; }
        public bool IsRawMaterial { get; set; }
        public bool IsFinishedGood { get; set; }
        public decimal StandardCost { get; set; }
    }

    public class ProductDetailVm : ProductListItemVm
    {
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? Notes { get; set; }
        public int BOMCount { get; set; }
    }
}
