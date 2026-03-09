using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagViewModelLayer.Asset;

namespace InTagLogicLayer.Asset
{
    public interface IAssetService
    {
        // CRUD
        Task<AssetDetailVm> GetByIdAsync(int id);
        Task<AssetListResultVm> GetAllAsync(AssetFilterVm filter);
        Task<AssetDetailVm> CreateAsync(AssetCreateVm model);
        Task<AssetDetailVm> UpdateAsync(int id, AssetUpdateVm model);
        Task SoftDeleteAsync(int id);

        // Lifecycle
        Task<AssetDetailVm> ChangeStatusAsync(int id, AssetStatus newStatus);
        Task<AssetTransferResultVm> TransferAsync(AssetTransferCreateVm model);

        // Depreciation
        Task<DepreciationResultVm> RunDepreciationAsync(int assetId, int fiscalYear, int fiscalMonth, decimal? unitsProduced = null);
        Task<int> RunBulkDepreciationAsync(int fiscalYear, int fiscalMonth);

        // Inspections
        Task<InspectionResultVm> CreateInspectionAsync(InspectionCreateVm model);

        // Lookups
        Task<IReadOnlyList<AssetTypeListVm>> GetAssetTypesAsync();
        Task<IReadOnlyList<LocationListVm>> GetLocationsAsync();
        Task<IReadOnlyList<DepartmentListVm>> GetDepartmentsAsync();
        Task<IReadOnlyList<VendorListVm>> GetVendorsAsync();

        // Depreciation — Advanced
        Task<IReadOnlyList<DepreciationResultVm>> GetDepreciationHistoryAsync(int assetId);
        Task<IReadOnlyList<DepreciationForecastVm>> ForecastDepreciationAsync(int assetId, int monthsAhead);
        Task<DepreciationSummaryVm> GetDepreciationSummaryAsync(int? fiscalYear = null);
        Task<BulkDepreciationResultVm> RunBulkDepreciationDetailedAsync(int fiscalYear, int fiscalMonth);

        // Dashboard & Reports
        Task<AssetDashboardVm> GetDashboardAsync();
        Task<byte[]> ExportAssetRegisterAsync(AssetFilterVm filter);
        Task<byte[]> ExportDepreciationScheduleAsync(int fiscalYear);
        Task<byte[]> ExportTCOReportAsync();

    }
}
