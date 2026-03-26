using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Manufacturing
{
    public class ProductionOrderRepository : GenericRepository<ProductionOrder>, IProductionOrderRepository
    {
        public ProductionOrderRepository(InTagDbContext context) : base(context) { }

        public async Task<ProductionOrder?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(o => o.Product)
                .Include(o => o.BOM).ThenInclude(b => b!.Lines).ThenInclude(l => l.ComponentProduct)
                .Include(o => o.Routing).ThenInclude(r => r!.Operations.OrderBy(op => op.Sequence))
                    .ThenInclude(op => op.WorkCenter)
                .Include(o => o.ProductionLogs.OrderByDescending(l => l.LogDate))
                .Include(o => o.LotBatches)
                .Include(o => o.QualityChecks.OrderByDescending(q => q.CheckDate))
                .FirstOrDefaultAsync(o => o.Id == id);



        public async Task<IReadOnlyList<ProductionOrder>> GetByStatusAsync(ProductionOrderStatus status)
            => await _dbSet.Where(o => o.Status == status)
                .Include(o => o.Product)
                .OrderBy(o => o.PlannedStartDate)
                .ToListAsync();

        public async Task<bool> OrderNumberExistsAsync(string orderNumber)
            => await _dbSet.AnyAsync(o => o.OrderNumber == orderNumber);
    }
}
