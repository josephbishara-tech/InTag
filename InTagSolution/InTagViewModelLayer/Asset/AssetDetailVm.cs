using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class AssetDetailVm
    {
        public int Id { get; set; }
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public AssetCategory Category { get; set; }
        public AssetStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string CategoryDisplay => Category.ToString();

        // Financial
        public decimal PurchaseCost { get; set; }
        public decimal? SalvageValue { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public int UsefulLifeMonths { get; set; }
        public decimal CurrentBookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }

        // Dates
        public DateTimeOffset? AcquisitionDate { get; set; }
        public DateTimeOffset? WarrantyStartDate { get; set; }
        public DateTimeOffset? WarrantyEndDate { get; set; }
        public bool IsUnderWarranty => WarrantyEndDate.HasValue && WarrantyEndDate > DateTimeOffset.UtcNow;

        // Identification
        public string? SerialNumber { get; set; }
        public string? Barcode { get; set; }
        public string? Manufacturer { get; set; }
        public string? ModelNumber { get; set; }

        // Related
        public string? AssetTypeName { get; set; }
        public string? LocationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? VendorName { get; set; }
        public string? ParentAssetName { get; set; }
        public int? ParentAssetId { get; set; }
        public int ChildAssetCount { get; set; }

        public string? Notes { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
