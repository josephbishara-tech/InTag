using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Inventory
{
    /// <summary>
    /// Stock level per product per warehouse per bin.
    /// </summary>
    public class StockItem : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public int? StorageBinId { get; set; }

        public decimal QuantityOnHand { get; set; }

        public decimal QuantityReserved { get; set; }

        public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

        [Required]
        public StockStatus Status { get; set; } = StockStatus.Available;

        // ── Reorder Management ───────────────
        public decimal MinimumLevel { get; set; }

        public decimal MaximumLevel { get; set; }

        public decimal ReorderPoint { get; set; }

        public decimal ReorderQuantity { get; set; }

        /// <summary>
        /// Economic Order Quantity
        /// </summary>
        public decimal? EOQ { get; set; }

        // ── Valuation ────────────────────────
        [Required]
        public ValuationMethod ValuationMethod { get; set; } = ValuationMethod.WeightedAverage;

        public decimal UnitCost { get; set; }

        public decimal TotalValue => QuantityOnHand * UnitCost;

        // ── Lot/Serial link ──────────────────
        [MaxLength(50)]
        public string? LotNumber { get; set; }

        [MaxLength(50)]
        public string? SerialNumber { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        // Navigation
        public Manufacturing.Product Product { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public StorageBin? StorageBin { get; set; }
    }
}
