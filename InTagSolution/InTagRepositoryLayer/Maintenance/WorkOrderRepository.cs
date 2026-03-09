using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Maintenance;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Maintenance
{
    public class WorkOrderRepository : GenericRepository<WorkOrder>, IWorkOrderRepository
    {
        public WorkOrderRepository(InTagDbContext context) : base(context) { }

        public async Task<WorkOrder?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(w => w.Asset)
                .Include(w => w.PMSchedule)
                .Include(w => w.LaborEntries.OrderByDescending(l => l.StartTime))
                .Include(w => w.Parts)
                .FirstOrDefaultAsync(w => w.Id == id);

        public async Task<IReadOnlyList<WorkOrder>> GetByAssetAsync(int assetId)
            => await _dbSet.Where(w => w.AssetId == assetId)
                .Include(w => w.Asset)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();

        public async Task<IReadOnlyList<WorkOrder>> GetOverdueAsync()
            => await _dbSet
                .Where(w => w.DueDate.HasValue && w.DueDate < DateTimeOffset.UtcNow
                            && w.Status != WorkOrderStatus.Completed
                            && w.Status != WorkOrderStatus.Closed
                            && w.Status != WorkOrderStatus.Cancelled)
                .Include(w => w.Asset)
                .OrderBy(w => w.DueDate)
                .ToListAsync();

        public async Task<bool> WorkOrderNumberExistsAsync(string number)
            => await _dbSet.AnyAsync(w => w.WorkOrderNumber == number);
    }
}
