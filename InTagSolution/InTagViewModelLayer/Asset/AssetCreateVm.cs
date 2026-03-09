using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class AssetCreateVm
    {
        [Required, MaxLength(50)]
        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public AssetCategory Category { get; set; }

        [Required]
        [Display(Name = "Asset Type")]
        public int AssetTypeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Purchase cost must be greater than zero.")]
        [Display(Name = "Purchase Cost")]
        public decimal PurchaseCost { get; set; }

        [Display(Name = "Salvage Value")]
        public decimal? SalvageValue { get; set; }

        [Required]
        [Display(Name = "Depreciation Method")]
        public DepreciationMethod DepreciationMethod { get; set; }

        [Required]
        [Range(1, 1200, ErrorMessage = "Useful life must be between 1 and 1200 months.")]
        [Display(Name = "Useful Life (Months)")]
        public int UsefulLifeMonths { get; set; }

        [Display(Name = "Acquisition Date")]
        public DateTimeOffset? AcquisitionDate { get; set; }

        [MaxLength(100)]
        [Display(Name = "Purchase Order #")]
        public string? PurchaseOrderNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        [Display(Name = "Model Number")]
        public string? ModelNumber { get; set; }

        [Display(Name = "Warranty Start")]
        public DateTimeOffset? WarrantyStartDate { get; set; }

        [Display(Name = "Warranty End")]
        public DateTimeOffset? WarrantyEndDate { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Vendor")]
        public int? VendorId { get; set; }

        [Display(Name = "Parent Asset")]
        public int? ParentAssetId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
