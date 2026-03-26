using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Manufacturing
{
    public interface IWorkCenterRepository : IGenericRepository<WorkCenter>
    {
        Task<WorkCenter?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<WorkCenter>> GetActiveAsync();
    }
}
