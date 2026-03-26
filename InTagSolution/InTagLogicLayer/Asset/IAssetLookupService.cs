using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public interface IAssetLookupService
    {
        // Asset Types
        Task<IReadOnlyList<AssetTypeListVm>> GetAssetTypesAsync();
        Task<AssetTypeUpdateVm> GetAssetTypeByIdAsync(int id);
        Task<AssetTypeListVm> CreateAssetTypeAsync(AssetTypeCreateVm model);
        Task UpdateAssetTypeAsync(AssetTypeUpdateVm model);

        // Locations
        Task<IReadOnlyList<LocationListVm>> GetLocationsAsync();
        Task<LocationUpdateVm> GetLocationByIdAsync(int id);
        Task<LocationListVm> CreateLocationAsync(LocationCreateVm model);
        Task UpdateLocationAsync(LocationUpdateVm model);

        // Departments
        Task<IReadOnlyList<DepartmentListVm>> GetDepartmentsAsync();
        Task<DepartmentUpdateVm> GetDepartmentByIdAsync(int id);
        Task<DepartmentListVm> CreateDepartmentAsync(DepartmentCreateVm model);
        Task UpdateDepartmentAsync(DepartmentUpdateVm model);

        // Vendors
        Task<IReadOnlyList<VendorListVm>> GetVendorsAsync();
        Task<VendorUpdateVm> GetVendorByIdAsync(int id);
        Task<VendorListVm> CreateVendorAsync(VendorCreateVm model);
        Task UpdateVendorAsync(VendorUpdateVm model);
    }
}
