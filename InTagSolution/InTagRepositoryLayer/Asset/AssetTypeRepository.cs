using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Asset
{
    public class AssetTypeRepository : GenericRepository<AssetType>, IAssetTypeRepository
    {
        public AssetTypeRepository(InTagDbContext context) : base(context) { }

        public async Task<IReadOnlyList<AssetType>> GetByCategoryAsync(AssetCategory category)
            => await _dbSet.Where(t => t.Category == category).ToListAsync();

        public async Task<bool> NameExistsAsync(string name)
            => await _dbSet.AnyAsync(t => t.Name == name);
    }
}
