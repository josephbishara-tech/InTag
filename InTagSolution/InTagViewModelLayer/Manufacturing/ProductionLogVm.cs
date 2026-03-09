using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Manufacturing
{
    public class ProductionLogCreateVm
    {
        [Required]
        public int ProductionOrderId { get; set; }

        public int? RoutingOperationId { get; set; }

        public int? WorkCenterId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Quantity Produced")]
        public decimal QuantityProduced { get; set; }

        [Display(Name = "Quantity Scrapped")]
        public decimal QuantityScrapped { get; set; }

        [Display(Name = "Quantity Rework")]
        public decimal QuantityRework { get; set; }

        [Display(Name = "Setup Time (min)")]
        public decimal? SetupTimeActual { get; set; }

        [Display(Name = "Run Time (min)")]
        public decimal? RunTimeActual { get; set; }

        [Display(Name = "Downtime (min)")]
        public decimal? DowntimeMinutes { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ProductionLogVm
    {
        public int Id { get; set; }
        public string? OperationName { get; set; }
        public string? WorkCenterName { get; set; }
        public decimal QuantityProduced { get; set; }
        public decimal QuantityScrapped { get; set; }
        public decimal QuantityRework { get; set; }
        public DateTimeOffset LogDate { get; set; }
        public decimal? SetupTimeActual { get; set; }
        public decimal? RunTimeActual { get; set; }
        public decimal? DowntimeMinutes { get; set; }
        public string? Notes { get; set; }
    }
}
