namespace InTagViewModelLayer.Maintenance
{
    public class MTBFMTTRResultVm
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;

        /// <summary>Mean Time Between Failures (hours)</summary>
        public decimal MTBF { get; set; }

        /// <summary>Mean Time To Repair (hours)</summary>
        public decimal MTTR { get; set; }

        /// <summary>Availability = MTBF / (MTBF + MTTR)</summary>
        public decimal Availability { get; set; }

        public int FailureCount { get; set; }
        public decimal TotalDowntimeHours { get; set; }
        public decimal TotalOperatingHours { get; set; }
    }
}
