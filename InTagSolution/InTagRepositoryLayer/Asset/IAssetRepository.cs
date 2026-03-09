using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Asset
{
    public interface IAssetRepository : IGenericRepository<AssetItem>
    {
        Task<AssetItem?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<AssetItem>> GetByStatusAsync(AssetStatus status);
        Task<IReadOnlyList<AssetItem>> GetByLocationAsync(int locationId);
        Task<IReadOnlyList<AssetItem>> GetByDepartmentAsync(int departmentId);
        Task<IReadOnlyList<AssetItem>> GetChildAssetsAsync(int parentAssetId);
        Task<bool> AssetCodeExistsAsync(string assetCode);
        Task<IReadOnlyList<AssetItem>> SearchAsync(string searchTerm, int page, int pageSize);
        Task<int> SearchCountAsync(string searchTerm);
    }
}
