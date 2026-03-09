using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class LotBatchCreateVm
    {
        [Required]
        public int ProductId { get; set; }

        public int? ProductionOrderId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }

        [MaxLength(200)]
        [Display(Name = "Storage Location")]
        public string? StorageLocation { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class LotBatchListVm
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public LotBatchStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public DateTimeOffset ManufactureDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTimeOffset.UtcNow;
        public string? StorageLocation { get; set; }
        public int QualityCheckCount { get; set; }
    }
}
