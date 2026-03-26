using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class WorkCenterCreateVm
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Status")]
        public WorkCenterStatus Status { get; set; } = WorkCenterStatus.Active;

        [Range(0, double.MaxValue)]
        [Display(Name = "Capacity (units/hour)")]
        public decimal CapacityPerHour { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Labor Rate ($/hr)")]
        public decimal LaborRatePerHour { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Overhead Rate ($/hr)")]
        public decimal OverheadRatePerHour { get; set; }

        [Range(0, 24)]
        [Display(Name = "Available Hours/Day")]
        public decimal AvailableHoursPerDay { get; set; } = 8;

        [Range(0, 100)]
        [Display(Name = "Efficiency %")]
        public decimal EfficiencyPercent { get; set; } = 100;

        public int? LocationId { get; set; }

        public int? DepartmentId { get; set; }
    }

    public class WorkCenterUpdateVm
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Status")]
        public WorkCenterStatus Status { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Capacity (units/hour)")]
        public decimal CapacityPerHour { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Labor Rate ($/hr)")]
        public decimal LaborRatePerHour { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Overhead Rate ($/hr)")]
        public decimal OverheadRatePerHour { get; set; }

        [Range(0, 24)]
        [Display(Name = "Available Hours/Day")]
        public decimal AvailableHoursPerDay { get; set; }

        [Range(0, 100)]
        [Display(Name = "Efficiency %")]
        public decimal EfficiencyPercent { get; set; }

        public int? LocationId { get; set; }

        public int? DepartmentId { get; set; }
    }

    public class WorkCenterDetailVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public WorkCenterStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public decimal CapacityPerHour { get; set; }
        public decimal LaborRatePerHour { get; set; }
        public decimal OverheadRatePerHour { get; set; }
        public decimal TotalRatePerHour => LaborRatePerHour + OverheadRatePerHour;
        public decimal AvailableHoursPerDay { get; set; }
        public decimal EfficiencyPercent { get; set; }
        public string? LocationName { get; set; }
        public string? DepartmentName { get; set; }
        public int RoutingOperationCount { get; set; }
    }

    public class WorkCenterListItemVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public WorkCenterStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public decimal CapacityPerHour { get; set; }
        public decimal LaborRatePerHour { get; set; }
        public decimal OverheadRatePerHour { get; set; }
        public decimal TotalRatePerHour => LaborRatePerHour + OverheadRatePerHour;
        public decimal AvailableHoursPerDay { get; set; }
        public decimal EfficiencyPercent { get; set; }
    }
}
