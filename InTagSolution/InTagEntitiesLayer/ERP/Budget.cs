using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.ERP
{
    public class Budget : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public int FiscalYear { get; set; }

        public int Month { get; set; }

        [Required]
        public int AccountId { get; set; }

        public int? CostCenterId { get; set; }

        public decimal PlannedAmount { get; set; }

        public decimal ActualAmount { get; set; }

        public decimal Variance => PlannedAmount - ActualAmount;

        public decimal VariancePercent => PlannedAmount != 0
            ? Math.Round(Variance / PlannedAmount * 100, 2) : 0;

        // Navigation
        public Account Account { get; set; } = null!;
        public CostCenter? CostCenter { get; set; }
    }
}
