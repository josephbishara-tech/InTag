using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Inventory
{
    public class WarehouseCreateVm
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Address { get; set; }

        public int? LocationId { get; set; }
    }

    public class WarehouseListVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public int BinCount { get; set; }
        public int StockItemCount { get; set; }
    }

    public class StorageBinCreateVm
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "Bin Code")]
        public string BinCode { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Aisle { get; set; }

        [MaxLength(50)]
        public string? Shelf { get; set; }

        [MaxLength(50)]
        public string? Level { get; set; }
    }

    public class StorageBinListVm
    {
        public int Id { get; set; }
        public string BinCode { get; set; } = null!;
        public string? Aisle { get; set; }
        public string? Shelf { get; set; }
        public string? Level { get; set; }
        public string FullPath => string.Join("-", new[] { Aisle, Shelf, Level }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
