using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Inventory
{
    public class TransactionCreateVm
    {
        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        public int? StorageBinId { get; set; }

        [Display(Name = "To Warehouse")]
        public int? ToWarehouseId { get; set; }

        public int? ToStorageBinId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [MaxLength(200)]
        [Display(Name = "Reference #")]
        public string? ReferenceNumber { get; set; }

        [MaxLength(50)]
        [Display(Name = "Lot #")]
        public string? LotNumber { get; set; }

        [MaxLength(50)]
        [Display(Name = "Serial #")]
        public string? SerialNumber { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class TransactionListItemVm
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = null!;
        public TransactionType Type { get; set; }
        public string TypeDisplay => Type.ToString();
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string WarehouseCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? LotNumber { get; set; }
        public bool IsInbound => Type == TransactionType.Receipt || Type == TransactionType.Return || Type == TransactionType.ProductionOutput;
    }

    public class TransactionFilterVm
    {
        public string? SearchTerm { get; set; }
        public TransactionType? Type { get; set; }
        public int? ProductId { get; set; }
        public int? WarehouseId { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class TransactionListResultVm
    {
        public IReadOnlyList<TransactionListItemVm> Items { get; set; } = new List<TransactionListItemVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
