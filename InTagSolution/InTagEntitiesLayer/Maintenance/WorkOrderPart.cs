using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Maintenance
{
    public class WorkOrderPart : BaseEntity
    {
        [Required]
        public int WorkOrderId { get; set; }

        [Required, MaxLength(100)]
        public string PartNumber { get; set; } = null!;

        [Required, MaxLength(300)]
        public string PartName { get; set; } = null!;

        [Required]
        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalCost => Quantity * UnitCost;

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public WorkOrder WorkOrder { get; set; } = null!;
    }
}
