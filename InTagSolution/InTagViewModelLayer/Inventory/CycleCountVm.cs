using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Inventory
{
    public class CycleCountCreateVm
    {
        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CycleCountDetailVm
    {
        public int Id { get; set; }
        public string CountNumber { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public DateTimeOffset CountDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<CycleCountLineVm> Lines { get; set; } = new List<CycleCountLineVm>();
        public int TotalLines { get; set; }
        public int VarianceCount { get; set; }
        public decimal TotalVarianceValue { get; set; }
    }

    public class CycleCountLineVm
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? BinCode { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal CountedQuantity { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public bool IsAdjusted { get; set; }
    }

    public class CycleCountLineUpdateVm
    {
        [Required]
        public int CycleCountLineId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Counted Qty")]
        public decimal CountedQuantity { get; set; }
    }

    public class CycleCountListItemVm
    {
        public int Id { get; set; }
        public string CountNumber { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public DateTimeOffset CountDate { get; set; }
        public bool IsCompleted { get; set; }
        public int LineCount { get; set; }
    }
}
