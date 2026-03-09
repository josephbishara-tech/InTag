using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class AssetUpdateVm
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public AssetCategory Category { get; set; }

        [Required]
        public int AssetTypeId { get; set; }

        public decimal? SalvageValue { get; set; }

        public int? LocationId { get; set; }

        public int? DepartmentId { get; set; }

        public int? VendorId { get; set; }

        public int? ParentAssetId { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? ModelNumber { get; set; }

        public DateTimeOffset? WarrantyStartDate { get; set; }

        public DateTimeOffset? WarrantyEndDate { get; set; }

        [MaxLength(200)]
        public string? WarrantyTerms { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
