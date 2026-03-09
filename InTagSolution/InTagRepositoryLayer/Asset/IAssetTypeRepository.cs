using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Asset
{
    public interface IAssetTypeRepository : IGenericRepository<AssetType>
    {
        Task<IReadOnlyList<AssetType>> GetByCategoryAsync(AssetCategory category);
        Task<bool> NameExistsAsync(string name);
    }
}
