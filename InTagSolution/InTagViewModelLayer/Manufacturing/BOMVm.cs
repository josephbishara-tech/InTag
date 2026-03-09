using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class BOMCreateVm
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Output Quantity")]
        [Range(0.0001, double.MaxValue)]
        public decimal OutputQuantity { get; set; } = 1;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class BOMDetailVm
    {
        public int Id { get; set; }
        public string BOMCode { get; set; } = null!;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string Version { get; set; } = null!;
        public BOMStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public decimal OutputQuantity { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<BOMLineVm> Lines { get; set; } = new List<BOMLineVm>();
        public decimal TotalMaterialCost { get; set; }
    }

    public class BOMLineVm
    {
        public int Id { get; set; }
        public int ComponentProductId { get; set; }
        public string ComponentCode { get; set; } = null!;
        public string ComponentName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string UOM { get; set; } = null!;
        public decimal ScrapFactor { get; set; }
        public bool IsPhantom { get; set; }
        public decimal LineCost { get; set; }
    }

    public class BOMLineCreateVm
    {
        [Required]
        public int BOMId { get; set; }

        [Required]
        [Display(Name = "Component")]
        public int ComponentProductId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        public UnitOfMeasure UOM { get; set; }

        [Range(0, 100)]
        [Display(Name = "Scrap %")]
        public decimal ScrapFactor { get; set; }

        public bool IsPhantom { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class BOMListItemVm
    {
        public int Id { get; set; }
        public string BOMCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string Version { get; set; } = null!;
        public BOMStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public int LineCount { get; set; }
    }
}
