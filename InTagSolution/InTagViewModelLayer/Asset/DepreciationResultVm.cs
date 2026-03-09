namespace InTagViewModelLayer.Asset
{
    public class DepreciationResultVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string Period { get; set; } = null!;
        public decimal OpeningBookValue { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal ClosingBookValue { get; set; }
        public string Method { get; set; } = null!;
        public bool IsFullyDepreciated { get; set; }
    }
}
