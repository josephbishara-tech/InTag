using InTagEntitiesLayer.Enums;
using InTagViewModelLayer.Workflow;

namespace InTagLogicLayer.Workflow
{
    public interface IWorkflowService
    {
        // Definitions
        Task<WorkflowDefinitionDetailVm> GetDefinitionByIdAsync(int id);
        Task<IReadOnlyList<WorkflowDefinitionListVm>> GetDefinitionsAsync();
        Task<WorkflowDefinitionDetailVm> CreateDefinitionAsync(WorkflowDefinitionCreateVm model);
        Task<WorkflowDefinitionDetailVm> AddStepAsync(WorkflowStepCreateVm model);
        Task RemoveStepAsync(int stepId);
        Task<WorkflowDefinitionDetailVm> ActivateDefinitionAsync(int id);
        Task<WorkflowDefinitionDetailVm> DeactivateDefinitionAsync(int id);

        // Instances
        Task<WorkflowInstanceDetailVm> GetInstanceByIdAsync(int id);
        Task<WorkflowListResultVm> GetInstancesAsync(WorkflowFilterVm filter);
        Task<WorkflowInstanceDetailVm> StartWorkflowAsync(StartWorkflowVm model);
        Task<WorkflowInstanceDetailVm> StartWorkflowByCategoryAsync(WorkflowCategory category,
            string entityType, int entityId, string? entityReference);
        Task<WorkflowInstanceDetailVm> SubmitActionAsync(WorkflowActionSubmitVm model);
        Task<WorkflowInstanceDetailVm> CancelInstanceAsync(int id);

        // Pending
        Task<IReadOnlyList<WorkflowInstanceListVm>> GetPendingApprovalsAsync();
        // Escalation
        Task<EscalationResultVm> ProcessEscalationsAsync();

        // Templates
        Task<int> SeedTemplatesAsync();
    }
}
