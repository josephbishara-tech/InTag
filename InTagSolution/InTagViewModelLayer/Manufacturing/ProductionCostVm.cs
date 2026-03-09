namespace InTagViewModelLayer.Manufacturing
{
    public class ProductionCostResultVm
    {
        public string OrderNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal PlannedQuantity { get; set; }
        public decimal CompletedQuantity { get; set; }
        public decimal ScrapQuantity { get; set; }
        public decimal GoodUnits { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal TotalPlannedCost { get; set; }
        public decimal ActualLaborCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal PlannedUnitCost { get; set; }
        public decimal ActualUnitCost { get; set; }
        public decimal CostVariance { get; set; }
        public IReadOnlyList<CostLineVm> MaterialLines { get; set; } = new List<CostLineVm>();
        public IReadOnlyList<CostLineVm> LaborLines { get; set; } = new List<CostLineVm>();
    }

    public class CostLineVm
    {
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
