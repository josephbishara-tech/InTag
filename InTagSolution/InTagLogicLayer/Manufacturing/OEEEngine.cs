using InTagViewModelLayer.Manufacturing;

namespace InTagLogicLayer.Manufacturing
{
    /// <summary>
    /// Calculates Overall Equipment Effectiveness from production log data.
    /// OEE = Availability × Performance × Quality
    /// </summary>
    public static class OEEEngine
    {
        public static OEEResultVm Calculate(
            decimal plannedProductionMinutes,
            decimal actualRunMinutes,
            decimal totalDowntimeMinutes,
            decimal idealCycleTimePerUnit,
            decimal totalProduced,
            decimal scrapUnits)
        {
            // Availability = (Planned - Downtime) / Planned
            var availableTime = plannedProductionMinutes - totalDowntimeMinutes;
            var availability = plannedProductionMinutes > 0
                ? Math.Round(availableTime / plannedProductionMinutes * 100, 1)
                : 0;

            // Performance = (Ideal Cycle Time × Total Produced) / Actual Run Time
            var performance = actualRunMinutes > 0
                ? Math.Round(idealCycleTimePerUnit * totalProduced / actualRunMinutes * 100, 1)
                : 0;

            // Quality = Good Units / Total Produced
            var goodUnits = totalProduced - scrapUnits;
            var quality = totalProduced > 0
                ? Math.Round(goodUnits / totalProduced * 100, 1)
                : 0;

            // Cap at 100%
            availability = Math.Min(availability, 100);
            performance = Math.Min(performance, 100);
            quality = Math.Min(quality, 100);

            return new OEEResultVm
            {
                Availability = availability,
                Performance = performance,
                Quality = quality,
                PlannedProductionTime = plannedProductionMinutes,
                ActualRunTime = actualRunMinutes,
                TotalDowntime = totalDowntimeMinutes,
                IdealCycleTime = idealCycleTimePerUnit,
                TotalProduced = totalProduced,
                GoodUnits = goodUnits,
                ScrapUnits = scrapUnits
            };
        }
    }
}
