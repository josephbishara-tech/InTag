using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Inventory
{
    // ── ABC Analysis ─────────────────────
    public class ABCAnalysisVm
    {
        public decimal TotalInventoryValue { get; set; }
        public int TotalSKUs { get; set; }
        public IReadOnlyList<ABCCategoryVm> Categories { get; set; } = new List<ABCCategoryVm>();
        public IReadOnlyList<ABCItemVm> Items { get; set; } = new List<ABCItemVm>();
    }

    public class ABCCategoryVm
    {
        public string Category { get; set; } = null!; // A, B, C
        public int SKUs { get; set; }
        public decimal SKUPercent { get; set; }
        public decimal TotalValue { get; set; }
        public decimal ValuePercent { get; set; }
    }

    public class ABCItemVm
    {
        public int StockItemId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string WarehouseCode { get; set; } = null!;
        public decimal QuantityOnHand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public decimal CumulativePercent { get; set; }
        public string Category { get; set; } = null!; // A, B, C
        public int Rank { get; set; }
    }

    // ── Stock Aging ──────────────────────
    public class StockAgingVm
    {
        public IReadOnlyList<AgingBucketVm> Buckets { get; set; } = new List<AgingBucketVm>();
        public IReadOnlyList<AgingItemVm> Items { get; set; } = new List<AgingItemVm>();
        public decimal TotalValue { get; set; }
        public decimal SlowMovingValue { get; set; }
        public decimal SlowMovingPercent { get; set; }
    }

    public class AgingBucketVm
    {
        public string Label { get; set; } = null!;
        public int SKUs { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AgingItemVm
    {
        public int StockItemId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string WarehouseCode { get; set; } = null!;
        public decimal QuantityOnHand { get; set; }
        public decimal TotalValue { get; set; }
        public int DaysSinceLastMovement { get; set; }
        public DateTimeOffset? LastMovementDate { get; set; }
        public string AgingBucket { get; set; } = null!;
    }

    // ── Expiry Tracking ──────────────────
    public class ExpiryReportVm
    {
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public decimal ExpiredValue { get; set; }
        public IReadOnlyList<ExpiryItemVm> Items { get; set; } = new List<ExpiryItemVm>();
    }

    public class ExpiryItemVm
    {
        public int StockItemId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string WarehouseCode { get; set; } = null!;
        public string? LotNumber { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal TotalValue { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string Status { get; set; } = null!; // Expired, ExpiringSoon, OK
    }

    // ── Inventory Turnover ───────────────
    public class InventoryTurnoverVm
    {
        public decimal AverageInventoryValue { get; set; }
        public decimal COGSValue { get; set; }
        public decimal TurnoverRatio { get; set; }
        public decimal DaysOnHand { get; set; }
        public IReadOnlyList<TurnoverByProductVm> ByProduct { get; set; } = new List<TurnoverByProductVm>();
    }

    public class TurnoverByProductVm
    {
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal AverageStock { get; set; }
        public decimal TotalIssued { get; set; }
        public decimal TurnoverRatio { get; set; }
        public decimal DaysOnHand { get; set; }
        public string Performance { get; set; } = null!; // Fast, Normal, Slow, Dead
    }
}
