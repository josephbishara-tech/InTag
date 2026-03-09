using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    public class DepreciationRecord : BaseEntity
    {
        [Required]
        public int AssetId { get; set; }

        /// <summary>
        /// Period: e.g. "2026-03" for March 2026
        /// </summary>
        [Required, MaxLength(7)]
        public string Period { get; set; } = null!;

        [Required]
        public int FiscalYear { get; set; }

        [Required]
        public int FiscalMonth { get; set; }

        [Required]
        public DepreciationMethod Method { get; set; }

        /// <summary>
        /// Book value at start of this period
        /// </summary>
        [Required]
        public decimal OpeningBookValue { get; set; }

        /// <summary>
        /// Depreciation amount for this period
        /// </summary>
        [Required]
        public decimal DepreciationAmount { get; set; }

        /// <summary>
        /// Total accumulated depreciation after this period
        /// </summary>
        [Required]
        public decimal AccumulatedDepreciation { get; set; }

        /// <summary>
        /// Book value at end of this period
        /// </summary>
        [Required]
        public decimal ClosingBookValue { get; set; }

        /// <summary>
        /// For units-of-production method
        /// </summary>
        public decimal? UnitsProduced { get; set; }

        public bool IsPosted { get; set; }

        // Navigation
        public AssetItem Asset { get; set; } = null!;
    }
}
