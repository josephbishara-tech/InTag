using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class StockMove : BaseEntity
    {
        [Required, MaxLength(50)]
        public string MoveNumber { get; set; } = null!;

        [Required]
        public StockMoveType MoveType { get; set; }

        public StockMoveStatus Status { get; set; } = StockMoveStatus.Draft;

        [Required]
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public int SourceLocationId { get; set; }

        public int DestinationLocationId { get; set; }

        public DateTimeOffset ScheduledDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? CompletedDate { get; set; }

        [MaxLength(100)]
        public string? Reference { get; set; }

        /// <summary>
        /// Source document: SO-xxx, PO-xxx, MO-xxx
        /// </summary>
        [MaxLength(50)]
        public string? OriginDocument { get; set; }

        public int? SalesOrderId { get; set; }
        public int? PurchaseOrderId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public SalesOrder? SalesOrder { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public ICollection<StockMoveLine> MoveLines { get; set; } = new List<StockMoveLine>();
    }

    public class StockMoveLine : BaseEntity
    {
        [Required]
        public int StockMoveId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        [MaxLength(100)]
        public string? LotNumber { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        // Navigation
        public StockMove StockMove { get; set; } = null!;
    }
}
