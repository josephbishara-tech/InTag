using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Manufacturing
{
    public interface IProductionOrderRepository : IGenericRepository<ProductionOrder>
    {
        Task<ProductionOrder?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<ProductionOrder>> GetByStatusAsync(ProductionOrderStatus status);
        Task<bool> OrderNumberExistsAsync(string orderNumber);
    }
}
