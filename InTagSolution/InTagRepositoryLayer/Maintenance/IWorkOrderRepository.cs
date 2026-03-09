using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Maintenance;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Maintenance
{
    public interface IWorkOrderRepository : IGenericRepository<WorkOrder>
    {
        Task<WorkOrder?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<WorkOrder>> GetByAssetAsync(int assetId);
        Task<IReadOnlyList<WorkOrder>> GetOverdueAsync();
        Task<bool> WorkOrderNumberExistsAsync(string number);
    }
}
