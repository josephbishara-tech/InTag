using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Inventory
{
    public class StockItemDetailVm
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string UOM { get; set; } = null!;
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public string? BinCode { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable { get; set; }
        public StockStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public decimal MinimumLevel { get; set; }
        public decimal MaximumLevel { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal ReorderQuantity { get; set; }
        public ValuationMethod ValuationMethod { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public string? LotNumber { get; set; }
        public string? SerialNumber { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public ReorderStatus ReorderStatus { get; set; }
    }

    public class StockFilterVm
    {
        public string? SearchTerm { get; set; }
        public int? WarehouseId { get; set; }
        public int? ProductId { get; set; }
        public ReorderStatus? ReorderStatus { get; set; }
        public string? SortBy { get; set; } = "ProductCode";
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class StockListResultVm
    {
        public IReadOnlyList<StockItemDetailVm> Items { get; set; } = new List<StockItemDetailVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class StockLevelUpdateVm
    {
        public int StockItemId { get; set; }
        public decimal MinimumLevel { get; set; }
        public decimal MaximumLevel { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal ReorderQuantity { get; set; }
    }
}
