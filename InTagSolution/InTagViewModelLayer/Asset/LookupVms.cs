namespace InTagViewModelLayer.Asset
{
    public class AssetTypeListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
    }

    public class LocationListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
    }

    public class DepartmentListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
    }

    public class VendorListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
