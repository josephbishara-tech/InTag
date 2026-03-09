using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Workflow
{
    public class WorkflowRepository : GenericRepository<WorkflowInstance>, IWorkflowRepository
    {
        private readonly InTagDbContext _ctx;

        public WorkflowRepository(InTagDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<WorkflowInstance?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(i => i.WorkflowDefinition).ThenInclude(d => d.Steps.OrderBy(s => s.StepOrder))
                .Include(i => i.Actions.OrderBy(a => a.ActionDate))
                    .ThenInclude(a => a.WorkflowStep)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IReadOnlyList<WorkflowInstance>> GetPendingForUserAsync(Guid userId)
        {
            // Get instances where the current step's assignee role matches
            // or the specific user is assigned
            return await _dbSet
                .Include(i => i.WorkflowDefinition).ThenInclude(d => d.Steps)
                .Include(i => i.Actions)
                .Where(i => i.Status == WorkflowInstanceStatus.InProgress)
                .ToListAsync();
            // Note: further filtering by role done in service layer
        }

        public async Task<IReadOnlyList<WorkflowInstance>> GetByEntityAsync(string entityType, int entityId)
            => await _dbSet
                .Include(i => i.WorkflowDefinition)
                .Include(i => i.Actions).ThenInclude(a => a.WorkflowStep)
                .Where(i => i.EntityType == entityType && i.EntityId == entityId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

        public async Task<WorkflowDefinition?> GetDefinitionWithStepsAsync(int id)
            => await _ctx.WorkflowDefinitions
                .Include(d => d.Steps.OrderBy(s => s.StepOrder))
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<WorkflowDefinition?> GetActiveDefinitionByCategoryAsync(WorkflowCategory category)
            => await _ctx.WorkflowDefinitions
                .Include(d => d.Steps.OrderBy(s => s.StepOrder))
                .FirstOrDefaultAsync(d => d.Category == category && d.Status == WorkflowStatus.Active);
    }
}
