using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Inventory
{
    public class InventoryDashboardVm
    {
        // KPIs
        public decimal TotalInventoryValue { get; set; }
        public int TotalSKUs { get; set; }
        public int WarehouseCount { get; set; }
        public decimal TurnoverRatio { get; set; }
        public int TransactionsThisMonth { get; set; }

        // Alerts
        public int OutOfStockCount { get; set; }
        public int BelowReorderCount { get; set; }
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int SlowMovingSKUs { get; set; }
        public decimal SlowMovingValue { get; set; }

        public IReadOnlyList<StockAlertVm> StockAlerts { get; set; } = new List<StockAlertVm>();
        public IReadOnlyList<ExpiryAlertVm> ExpiryAlerts { get; set; } = new List<ExpiryAlertVm>();

        // Breakdowns
        public IReadOnlyList<ValueByWarehouseVm> ByWarehouse { get; set; } = new List<ValueByWarehouseVm>();
        public IReadOnlyList<TopValueItemVm> TopValueItems { get; set; } = new List<TopValueItemVm>();
        public IReadOnlyList<TransactionSummaryVm> TransactionSummary { get; set; } = new List<TransactionSummaryVm>();
        public IReadOnlyList<RecentTxnVm> RecentTransactions { get; set; } = new List<RecentTxnVm>();
    }

    public class StockAlertVm
    {
        public int StockItemId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string WarehouseCode { get; set; } = null!;
        public decimal QuantityOnHand { get; set; }
        public decimal ReorderPoint { get; set; }
        public ReorderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
    }

    public class ExpiryAlertVm
    {
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? LotNumber { get; set; }
        public decimal Quantity { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
    }

    public class ValueByWarehouseVm
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int SKUs { get; set; }
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopValueItemVm
    {
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class TransactionSummaryVm
    {
        public string Type { get; set; } = null!;
        public int Count { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class RecentTxnVm
    {
        public string Icon { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
    }
}
