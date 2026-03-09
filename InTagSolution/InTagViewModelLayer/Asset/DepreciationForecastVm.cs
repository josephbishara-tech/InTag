namespace InTagViewModelLayer.Asset
{
    public class DepreciationForecastVm
    {
        public string Period { get; set; } = null!;
        public decimal OpeningBookValue { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal ClosingBookValue { get; set; }
        public bool IsProjected { get; set; } = true;
    }

    public class DepreciationSummaryVm
    {
        public int FiscalYear { get; set; }
        public int TotalAssets { get; set; }
        public int DepreciableAssets { get; set; }
        public int FullyDepreciatedAssets { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public decimal TotalBookValue { get; set; }
        public decimal TotalAccumulatedDepreciation { get; set; }
        public decimal TotalDepreciationThisYear { get; set; }
        public IReadOnlyList<DepreciationByMethodVm> ByMethod { get; set; } = new List<DepreciationByMethodVm>();
        public IReadOnlyList<DepreciationByMonthVm> ByMonth { get; set; } = new List<DepreciationByMonthVm>();
        public IReadOnlyList<DepreciationByCategoryVm> ByCategory { get; set; } = new List<DepreciationByCategoryVm>();
    }

    public class DepreciationByMethodVm
    {
        public string Method { get; set; } = null!;
        public int AssetCount { get; set; }
        public decimal TotalDepreciation { get; set; }
    }

    public class DepreciationByMonthVm
    {
        public string Period { get; set; } = null!;
        public decimal TotalDepreciation { get; set; }
        public int AssetsProcessed { get; set; }
    }

    public class DepreciationByCategoryVm
    {
        public string Category { get; set; } = null!;
        public int AssetCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalBookValue { get; set; }
        public decimal TotalDepreciation { get; set; }
    }

    public class BulkDepreciationRunVm
    {
        public int FiscalYear { get; set; }
        public int FiscalMonth { get; set; }
    }

    public class BulkDepreciationResultVm
    {
        public int AssetsProcessed { get; set; }
        public int AssetsSkipped { get; set; }
        public int AssetsFailed { get; set; }
        public string Period { get; set; } = null!;
        public decimal TotalDepreciation { get; set; }
        public IReadOnlyList<BulkDepreciationItemVm> Details { get; set; } = new List<BulkDepreciationItemVm>();
    }

    public class BulkDepreciationItemVm
    {
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public decimal DepreciationAmount { get; set; }
        public decimal ClosingBookValue { get; set; }
        public string Status { get; set; } = null!; // "Processed", "Skipped", "Failed"
        public string? ErrorMessage { get; set; }
    }
}
