using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Asset
{
    public class AssetFilterVm
    {
        public string? SearchTerm { get; set; }
        public AssetStatus? Status { get; set; }
        public AssetCategory? Category { get; set; }
        public int? AssetTypeId { get; set; }
        public int? LocationId { get; set; }
        public int? DepartmentId { get; set; }
        public string? SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AssetListResultVm
    {
        public IReadOnlyList<AssetListItemVm> Items { get; set; } = new List<AssetListItemVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class AssetListItemVm
    {
        public int Id { get; set; }
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public AssetCategory Category { get; set; }
        public AssetStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public decimal PurchaseCost { get; set; }
        public decimal CurrentBookValue { get; set; }
        public string? AssetTypeName { get; set; }
        public string? LocationName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTimeOffset? AcquisitionDate { get; set; }
    }
}
