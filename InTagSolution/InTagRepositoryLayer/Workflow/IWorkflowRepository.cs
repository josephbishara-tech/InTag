using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Workflow
{
    public interface IWorkflowRepository : IGenericRepository<WorkflowInstance>
    {
        Task<WorkflowInstance?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<WorkflowInstance>> GetPendingForUserAsync(Guid userId);
        Task<IReadOnlyList<WorkflowInstance>> GetByEntityAsync(string entityType, int entityId);
        Task<WorkflowDefinition?> GetDefinitionWithStepsAsync(int id);
        Task<WorkflowDefinition?> GetActiveDefinitionByCategoryAsync(WorkflowCategory category);
    }
}
