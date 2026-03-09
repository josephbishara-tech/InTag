using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Inventory
{
    public class InventoryValuationVm
    {
        public decimal TotalValue { get; set; }
        public int TotalSKUs { get; set; }
        public IReadOnlyList<ValuationByWarehouseVm> ByWarehouse { get; set; } = new List<ValuationByWarehouseVm>();
        public IReadOnlyList<ValuationByProductVm> ByProduct { get; set; } = new List<ValuationByProductVm>();
    }

    public class ValuationByWarehouseVm
    {
        public string WarehouseCode { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public int SKUs { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ValuationByProductVm
    {
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal TotalQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public ValuationMethod Method { get; set; }
    }

    public class ReorderReportVm
    {
        public int BelowReorderCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int BelowMinimumCount { get; set; }
        public IReadOnlyList<ReorderItemVm> Items { get; set; } = new List<ReorderItemVm>();
    }

    public class ReorderItemVm
    {
        public int StockItemId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string WarehouseCode { get; set; } = null!;
        public decimal QuantityOnHand { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal MinimumLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal SuggestedOrderQty { get; set; }
        public ReorderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
    }
}
