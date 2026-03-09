using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Inventory
{
    public class InventoryTransaction : BaseEntity
    {
        [Required, MaxLength(50)]
        public string TransactionNumber { get; set; } = null!;

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public int? StorageBinId { get; set; }

        /// <summary>
        /// For transfers: destination warehouse
        /// </summary>
        public int? ToWarehouseId { get; set; }

        public int? ToStorageBinId { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalCost => Quantity * UnitCost;

        [Required]
        public DateTimeOffset TransactionDate { get; set; }

        [MaxLength(200)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(50)]
        public string? LotNumber { get; set; }

        [MaxLength(50)]
        public string? SerialNumber { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsPosted { get; set; } = true;

        // Navigation
        public Manufacturing.Product Product { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public Warehouse? ToWarehouse { get; set; }
    }
}
