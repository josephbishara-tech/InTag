using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;

namespace InTagLogicLayer.Asset
{
    /// <summary>
    /// Calculates depreciation using four methods:
    /// Straight-Line, Declining Balance, Sum-of-Years-Digits, Units-of-Production.
    /// </summary>
    public static class DepreciationEngine
    {
        /// <summary>
        /// Calculate monthly depreciation for an asset.
        /// </summary>
        public static DepreciationResult Calculate(
            DepreciationMethod method,
            decimal purchaseCost,
            decimal salvageValue,
            int usefulLifeMonths,
            decimal currentBookValue,
            decimal accumulatedDepreciation,
            int elapsedMonths,
            decimal? unitsProducedThisPeriod = null,
            decimal? totalEstimatedUnits = null)
        {
            // Guard: already fully depreciated
            if (currentBookValue <= salvageValue)
            {
                return new DepreciationResult
                {
                    DepreciationAmount = 0,
                    ClosingBookValue = currentBookValue,
                    AccumulatedDepreciation = accumulatedDepreciation
                };
            }

            var depreciableBase = purchaseCost - salvageValue;
            decimal depreciationAmount;

            switch (method)
            {
                case DepreciationMethod.StraightLine:
                    depreciationAmount = CalculateStraightLine(depreciableBase, usefulLifeMonths);
                    break;

                case DepreciationMethod.DecliningBalance:
                    depreciationAmount = CalculateDecliningBalance(
                        currentBookValue, salvageValue, usefulLifeMonths);
                    break;

                case DepreciationMethod.SumOfYearsDigits:
                    depreciationAmount = CalculateSumOfYearsDigits(
                        depreciableBase, usefulLifeMonths, elapsedMonths);
                    break;

                case DepreciationMethod.UnitsOfProduction:
                    if (unitsProducedThisPeriod == null || totalEstimatedUnits == null || totalEstimatedUnits == 0)
                        throw new InvalidOperationException(
                            "Units-of-production method requires unitsProducedThisPeriod and totalEstimatedUnits.");
                    depreciationAmount = CalculateUnitsOfProduction(
                        depreciableBase, unitsProducedThisPeriod.Value, totalEstimatedUnits.Value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method));
            }

            // Don't depreciate below salvage value
            var maxAllowed = currentBookValue - salvageValue;
            depreciationAmount = Math.Min(depreciationAmount, maxAllowed);
            depreciationAmount = Math.Max(depreciationAmount, 0);
            depreciationAmount = Math.Round(depreciationAmount, 2);

            return new DepreciationResult
            {
                DepreciationAmount = depreciationAmount,
                ClosingBookValue = currentBookValue - depreciationAmount,
                AccumulatedDepreciation = accumulatedDepreciation + depreciationAmount
            };
        }

        /// <summary>
        /// Straight-Line: equal amount every month.
        /// Formula: (Cost - Salvage) / UsefulLifeMonths
        /// </summary>
        private static decimal CalculateStraightLine(decimal depreciableBase, int usefulLifeMonths)
        {
            if (usefulLifeMonths <= 0) return 0;
            return Math.Round(depreciableBase / usefulLifeMonths, 2);
        }

        /// <summary>
        /// Double Declining Balance: accelerated, applies 2x straight-line rate to book value.
        /// Monthly rate = (2 / UsefulLifeMonths)
        /// Formula: CurrentBookValue * monthlyRate
        /// </summary>
        private static decimal CalculateDecliningBalance(
            decimal currentBookValue, decimal salvageValue, int usefulLifeMonths)
        {
            if (usefulLifeMonths <= 0) return 0;
            var monthlyRate = 2.0m / usefulLifeMonths;
            var amount = currentBookValue * monthlyRate;
            return Math.Round(amount, 2);
        }

        /// <summary>
        /// Sum-of-Years-Digits: accelerated, higher in early periods.
        /// Uses years for the fraction, then divides by 12 for monthly amount.
        /// Fraction for year N = RemainingYears / SumOfYears
        /// </summary>
        private static decimal CalculateSumOfYearsDigits(
            decimal depreciableBase, int usefulLifeMonths, int elapsedMonths)
        {
            var usefulLifeYears = (int)Math.Ceiling(usefulLifeMonths / 12.0);
            if (usefulLifeYears <= 0) return 0;

            // Sum of years digits: n*(n+1)/2
            var sumOfYears = usefulLifeYears * (usefulLifeYears + 1) / 2;

            // Which year are we in? (1-based)
            var currentYear = (int)Math.Floor(elapsedMonths / 12.0) + 1;
            if (currentYear > usefulLifeYears) return 0;

            var remainingYears = usefulLifeYears - currentYear + 1;
            var annualDepreciation = depreciableBase * remainingYears / sumOfYears;
            var monthlyDepreciation = annualDepreciation / 12;

            return Math.Round(monthlyDepreciation, 2);
        }

        /// <summary>
        /// Units-of-Production: based on actual usage.
        /// Formula: (Cost - Salvage) * (UnitsThisPeriod / TotalEstimatedUnits)
        /// </summary>
        private static decimal CalculateUnitsOfProduction(
            decimal depreciableBase, decimal unitsProduced, decimal totalEstimatedUnits)
        {
            if (totalEstimatedUnits <= 0) return 0;
            var amount = depreciableBase * (unitsProduced / totalEstimatedUnits);
            return Math.Round(amount, 2);
        }
    }

    public class DepreciationResult
    {
        public decimal DepreciationAmount { get; set; }
        public decimal ClosingBookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
    }
}
