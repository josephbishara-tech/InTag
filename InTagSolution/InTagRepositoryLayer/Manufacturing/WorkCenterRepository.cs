using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Manufacturing
{
    public class WorkCenterRepository : GenericRepository<WorkCenter>, IWorkCenterRepository
    {
        public WorkCenterRepository(InTagDbContext context) : base(context) { }

        public async Task<WorkCenter?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(w => w.Location)
                .Include(w => w.Department)
                .Include(w => w.Operations)
                .FirstOrDefaultAsync(w => w.Id == id);

        public async Task<IReadOnlyList<WorkCenter>> GetActiveAsync()
            => await _dbSet
                .Where(w => w.Status == WorkCenterStatus.Active)
                .OrderBy(w => w.Code)
                .ToListAsync();
    }
}
