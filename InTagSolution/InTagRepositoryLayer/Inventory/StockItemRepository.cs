using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Inventory;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Inventory
{
    public class StockItemRepository : GenericRepository<StockItem>, IStockItemRepository
    {
        public StockItemRepository(InTagDbContext context) : base(context) { }

        public async Task<StockItem?> GetStockAsync(int productId, int warehouseId, int? binId = null, string? lotNumber = null)
            => await _dbSet
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .Include(s => s.StorageBin)
                .FirstOrDefaultAsync(s => s.ProductId == productId
                                          && s.WarehouseId == warehouseId
                                          && s.StorageBinId == binId
                                          && s.LotNumber == lotNumber);

        public async Task<IReadOnlyList<StockItem>> GetByWarehouseAsync(int warehouseId)
            => await _dbSet.Where(s => s.WarehouseId == warehouseId)
                .Include(s => s.Product).Include(s => s.StorageBin)
                .OrderBy(s => s.Product.ProductCode).ToListAsync();

        public async Task<IReadOnlyList<StockItem>> GetByProductAsync(int productId)
            => await _dbSet.Where(s => s.ProductId == productId)
                .Include(s => s.Warehouse).Include(s => s.StorageBin)
                .ToListAsync();

        public async Task<IReadOnlyList<StockItem>> GetBelowReorderPointAsync()
            => await _dbSet
                .Where(s => s.ReorderPoint > 0 && s.QuantityOnHand <= s.ReorderPoint)
                .Include(s => s.Product).Include(s => s.Warehouse)
                .OrderBy(s => s.QuantityOnHand).ToListAsync();

        public async Task<IReadOnlyList<StockItem>> GetOutOfStockAsync()
            => await _dbSet.Where(s => s.QuantityOnHand <= 0)
                .Include(s => s.Product).Include(s => s.Warehouse)
                .ToListAsync();
    }
}
