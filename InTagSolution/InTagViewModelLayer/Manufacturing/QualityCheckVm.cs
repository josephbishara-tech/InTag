using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Manufacturing
{
    public class QualityCheckCreateVm
    {
        public int? ProductionOrderId { get; set; }

        public int? LotBatchId { get; set; }

        public int? RoutingOperationId { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Check Name")]
        public string CheckName { get; set; } = null!;

        [MaxLength(500)]
        public string? Specification { get; set; }

        [MaxLength(200)]
        [Display(Name = "Actual Value")]
        public string? ActualValue { get; set; }

        [Required]
        public QualityCheckResult Result { get; set; }

        [MaxLength(1000)]
        public string? Findings { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Corrective Action")]
        public string? CorrectiveAction { get; set; }
    }

    public class QualityCheckVm
    {
        public int Id { get; set; }
        public string CheckName { get; set; } = null!;
        public string? Specification { get; set; }
        public string? ActualValue { get; set; }
        public QualityCheckResult Result { get; set; }
        public string ResultDisplay => Result.ToString();
        public DateTimeOffset CheckDate { get; set; }
        public string? Findings { get; set; }
        public string? CorrectiveAction { get; set; }
    }
}
