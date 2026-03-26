using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    //public class AssetTypeListVm
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = null!;
    //    public string Category { get; set; } = null!;
    //}

    //public class LocationListVm
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = null!;
    //    public string? Code { get; set; }
    //}

    //public class DepartmentListVm
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = null!;
    //    public string? Code { get; set; }
    //}

    //public class VendorListVm
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; } = null!;
    //}


        // ══════════════════════════════════════
        //  ASSET TYPES
        // ══════════════════════════════════════

        public class AssetTypeCreateVm
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(500)]
            public string? Description { get; set; }

            [Required]
            [Display(Name = "Depreciation Method")]
            public DepreciationMethod DefaultDepreciationMethod { get; set; }

            [Required]
            [Range(1, 1200)]
            [Display(Name = "Useful Life (months)")]
            public int UsefulLifeMonths { get; set; }

            [Required]
            public AssetCategory Category { get; set; }

            [Range(0, 100)]
            [Display(Name = "Salvage Value %")]
            public decimal DefaultSalvageValuePercent { get; set; }
        }

        public class AssetTypeUpdateVm
        {
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(500)]
            public string? Description { get; set; }

            [Required]
            [Display(Name = "Depreciation Method")]
            public DepreciationMethod DefaultDepreciationMethod { get; set; }

            [Required]
            [Range(1, 1200)]
            [Display(Name = "Useful Life (months)")]
            public int UsefulLifeMonths { get; set; }

            [Required]
            public AssetCategory Category { get; set; }

            [Range(0, 100)]
            [Display(Name = "Salvage Value %")]
            public decimal DefaultSalvageValuePercent { get; set; }
        }

        public class AssetTypeListVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DepreciationMethod DefaultDepreciationMethod { get; set; }
            public string DepreciationDisplay => DefaultDepreciationMethod.ToString();
            public int UsefulLifeMonths { get; set; }
            public AssetCategory Category { get; set; }
            public string CategoryDisplay => Category.ToString();
            public decimal DefaultSalvageValuePercent { get; set; }
        }

        // ══════════════════════════════════════
        //  LOCATIONS
        // ══════════════════════════════════════

        public class LocationCreateVm
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(20)]
            public string? Code { get; set; }

            [MaxLength(500)]
            public string? Address { get; set; }

            [MaxLength(100)]
            public string? Building { get; set; }

            [MaxLength(50)]
            public string? Floor { get; set; }

            [MaxLength(50)]
            public string? Room { get; set; }

            [Display(Name = "Parent Location")]
            public int? ParentLocationId { get; set; }
        }

        public class LocationUpdateVm
        {
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(20)]
            public string? Code { get; set; }

            [MaxLength(500)]
            public string? Address { get; set; }

            [MaxLength(100)]
            public string? Building { get; set; }

            [MaxLength(50)]
            public string? Floor { get; set; }

            [MaxLength(50)]
            public string? Room { get; set; }

            [Display(Name = "Parent Location")]
            public int? ParentLocationId { get; set; }
        }

        public class LocationListVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Code { get; set; }
            public string? Address { get; set; }
            public string? Building { get; set; }
            public string? Floor { get; set; }
            public string? Room { get; set; }
            public string? ParentLocationName { get; set; }
            public string FullPath => string.Join(" / ", new[] { Building, Floor, Room }.Where(s => !string.IsNullOrEmpty(s)));
        }

        // ══════════════════════════════════════
        //  DEPARTMENTS
        // ══════════════════════════════════════

        public class DepartmentCreateVm
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(20)]
            public string? Code { get; set; }

            [MaxLength(500)]
            public string? Description { get; set; }

            [Display(Name = "Parent Department")]
            public int? ParentDepartmentId { get; set; }
        }

        public class DepartmentUpdateVm
        {
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(20)]
            public string? Code { get; set; }

            [MaxLength(500)]
            public string? Description { get; set; }

            [Display(Name = "Parent Department")]
            public int? ParentDepartmentId { get; set; }
        }

        public class DepartmentListVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Code { get; set; }
            public string? Description { get; set; }
            public string? ParentDepartmentName { get; set; }
        }

        // ══════════════════════════════════════
        //  VENDORS
        // ══════════════════════════════════════

        public class VendorCreateVm
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(20)]
            public string? Code { get; set; }

            [MaxLength(200)]
            [Display(Name = "Contact Person")]
            public string? ContactPerson { get; set; }

            [MaxLength(100)]
            [EmailAddress]
            public string? Email { get; set; }

            [MaxLength(30)]
            public string? Phone { get; set; }

            [MaxLength(500)]
            public string? Address { get; set; }

            [MaxLength(200)]
            public string? Website { get; set; }

            [MaxLength(1000)]
            public string? Notes { get; set; }
        }

        public class VendorUpdateVm
        {
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; } = null!;

            [MaxLength(20)]
            public string? Code { get; set; }

            [MaxLength(200)]
            [Display(Name = "Contact Person")]
            public string? ContactPerson { get; set; }

            [MaxLength(100)]
            [EmailAddress]
            public string? Email { get; set; }

            [MaxLength(30)]
            public string? Phone { get; set; }

            [MaxLength(500)]
            public string? Address { get; set; }

            [MaxLength(200)]
            public string? Website { get; set; }

            [MaxLength(1000)]
            public string? Notes { get; set; }
        }

        public class VendorListVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Code { get; set; }
            public string? ContactPerson { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Website { get; set; }
        }
 


}
