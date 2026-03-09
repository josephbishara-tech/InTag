using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Maintenance
{
    public class MeterReadingVm
    {
        [Required]
        public int PMScheduleId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Current Reading")]
        public decimal CurrentReading { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class MeterReadingResultVm
    {
        public int PMScheduleId { get; set; }
        public string ScheduleName { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public string MeterType { get; set; } = null!;
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal NextThreshold { get; set; }
        public bool ThresholdReached { get; set; }
        public string? GeneratedWorkOrderNumber { get; set; }
    }
}
