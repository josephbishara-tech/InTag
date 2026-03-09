using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Asset
{
    public class AssetRepository : GenericRepository<AssetItem>, IAssetRepository
    {
        public AssetRepository(InTagDbContext context) : base(context) { }

        public async Task<AssetItem?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .Include(a => a.Department)
                .Include(a => a.Vendor)
                .Include(a => a.ParentAsset)
                .Include(a => a.ChildAssets)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IReadOnlyList<AssetItem>> GetByStatusAsync(AssetStatus status)
            => await _dbSet.Where(a => a.Status == status)
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .ToListAsync();

        public async Task<IReadOnlyList<AssetItem>> GetByLocationAsync(int locationId)
            => await _dbSet.Where(a => a.LocationId == locationId)
                .Include(a => a.AssetType)
                .ToListAsync();

        public async Task<IReadOnlyList<AssetItem>> GetByDepartmentAsync(int departmentId)
            => await _dbSet.Where(a => a.DepartmentId == departmentId)
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .ToListAsync();

        public async Task<IReadOnlyList<AssetItem>> GetChildAssetsAsync(int parentAssetId)
            => await _dbSet.Where(a => a.ParentAssetId == parentAssetId)
                .Include(a => a.AssetType)
                .ToListAsync();

        public async Task<bool> AssetCodeExistsAsync(string assetCode)
            => await _dbSet.AnyAsync(a => a.AssetCode == assetCode);

        public async Task<IReadOnlyList<AssetItem>> SearchAsync(
            string searchTerm, int page, int pageSize)
        {
            var query = _dbSet
                .Where(a => a.AssetCode.Contains(searchTerm)
                            || a.Name.Contains(searchTerm)
                            || (a.SerialNumber != null && a.SerialNumber.Contains(searchTerm))
                            || (a.Barcode != null && a.Barcode.Contains(searchTerm)))
                .Include(a => a.AssetType)
                .Include(a => a.Location)
                .Include(a => a.Department)
                .OrderBy(a => a.Name);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> SearchCountAsync(string searchTerm)
        {
            return await _dbSet.CountAsync(a =>
                a.AssetCode.Contains(searchTerm)
                || a.Name.Contains(searchTerm)
                || (a.SerialNumber != null && a.SerialNumber.Contains(searchTerm))
                || (a.Barcode != null && a.Barcode.Contains(searchTerm)));
        }
    }
}
