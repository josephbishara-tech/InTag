using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class RoutingCreateVm
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RoutingDetailVm
    {
        public int Id { get; set; }
        public string RoutingCode { get; set; } = null!;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Version { get; set; } = null!;
        public bool IsActive { get; set; }
        public decimal TotalCycleTimeMinutes { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<RoutingOperationVm> Operations { get; set; } = new List<RoutingOperationVm>();
    }

    public class RoutingOperationVm
    {
        public int Id { get; set; }
        public int Sequence { get; set; }
        public string OperationName { get; set; } = null!;
        public int WorkCenterId { get; set; }
        public string WorkCenterName { get; set; } = null!;
        public decimal SetupTimeMinutes { get; set; }
        public decimal RunTimePerUnitMinutes { get; set; }
        public int OverlapQuantity { get; set; }
        public string? Instructions { get; set; }
    }

    public class RoutingOperationCreateVm
    {
        [Required]
        public int RoutingId { get; set; }

        [Required]
        [Range(1, 999)]
        public int Sequence { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Operation")]
        public string OperationName { get; set; } = null!;

        [Required]
        [Display(Name = "Work Center")]
        public int WorkCenterId { get; set; }

        [Display(Name = "Setup (min)")]
        public decimal SetupTimeMinutes { get; set; }

        [Display(Name = "Run/Unit (min)")]
        public decimal RunTimePerUnitMinutes { get; set; }

        [Display(Name = "Overlap Qty")]
        public int OverlapQuantity { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }
    }
}
