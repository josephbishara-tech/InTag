using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Asset
{
    /// <summary>
    /// Core asset entity — named AssetItem to avoid conflict with C# keyword.
    /// Maps to table "Asset".
    /// </summary>
    public class AssetItem : BaseEntity
    {
        [Required, MaxLength(50)]
        public string AssetCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public AssetCategory Category { get; set; }

        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Draft;

        // ── Acquisition ──────────────────────
        public DateTimeOffset? AcquisitionDate { get; set; }

        [Required]
        public decimal PurchaseCost { get; set; }

        public decimal? SalvageValue { get; set; }

        [MaxLength(100)]
        public string? PurchaseOrderNumber { get; set; }

        // ── Depreciation ─────────────────────
        [Required]
        public DepreciationMethod DepreciationMethod { get; set; }

        /// <summary>
        /// Useful life in months
        /// </summary>
        [Required]
        public int UsefulLifeMonths { get; set; }

        public decimal CurrentBookValue { get; set; }

        public decimal AccumulatedDepreciation { get; set; }

        // ── Warranty ─────────────────────────
        public DateTimeOffset? WarrantyStartDate { get; set; }

        public DateTimeOffset? WarrantyEndDate { get; set; }

        [MaxLength(200)]
        public string? WarrantyTerms { get; set; }

        // ── Identification ───────────────────
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(200)]
        public string? QrCodeData { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? ModelNumber { get; set; }

        // ── Hierarchy ────────────────────────
        public int? ParentAssetId { get; set; }

        // ── Foreign Keys ─────────────────────
        [Required]
        public int AssetTypeId { get; set; }

        public int? LocationId { get; set; }

        public int? DepartmentId { get; set; }

        public int? VendorId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // ── Navigation ───────────────────────
        public AssetItem? ParentAsset { get; set; }
        public ICollection<AssetItem> ChildAssets { get; set; } = new List<AssetItem>();
        public AssetType AssetType { get; set; } = null!;
        public Location? Location { get; set; }
        public Department? Department { get; set; }
        public Vendor? Vendor { get; set; }
        public ICollection<DepreciationRecord> DepreciationRecords { get; set; } = new List<DepreciationRecord>();
        public ICollection<AssetTransfer> TransfersFrom { get; set; } = new List<AssetTransfer>();
        public ICollection<AssetTransfer> TransfersTo { get; set; } = new List<AssetTransfer>();
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}
