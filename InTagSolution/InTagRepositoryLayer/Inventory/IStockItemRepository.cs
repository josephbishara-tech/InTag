using InTagEntitiesLayer.Inventory;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Inventory
{
    public interface IStockItemRepository : IGenericRepository<StockItem>
    {
        Task<StockItem?> GetStockAsync(int productId, int warehouseId, int? binId = null, string? lotNumber = null);
        Task<IReadOnlyList<StockItem>> GetByWarehouseAsync(int warehouseId);
        Task<IReadOnlyList<StockItem>> GetByProductAsync(int productId);
        Task<IReadOnlyList<StockItem>> GetBelowReorderPointAsync();
        Task<IReadOnlyList<StockItem>> GetOutOfStockAsync();
    }
}
