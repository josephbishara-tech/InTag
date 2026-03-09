namespace InTagViewModelLayer.Manufacturing
{
    /// <summary>
    /// Overall Equipment Effectiveness: Availability × Performance × Quality
    /// </summary>
    public class OEEResultVm
    {
        public decimal Availability { get; set; }
        public decimal Performance { get; set; }
        public decimal Quality { get; set; }
        public decimal OEE => Math.Round(Availability * Performance * Quality / 10000, 1);

        // Raw data
        public decimal PlannedProductionTime { get; set; }
        public decimal ActualRunTime { get; set; }
        public decimal TotalDowntime { get; set; }
        public decimal IdealCycleTime { get; set; }
        public decimal TotalProduced { get; set; }
        public decimal GoodUnits { get; set; }
        public decimal ScrapUnits { get; set; }
    }
}
