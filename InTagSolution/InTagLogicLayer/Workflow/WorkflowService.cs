using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Workflow;


namespace InTagLogicLayer.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITenantService _tenantService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<WorkflowService> _logger;
        private readonly ILoggerFactory _loggerFactory;


        public WorkflowService(IUnitOfWork uow, ITenantService tenantService,
            INotificationService notificationService, ILogger<WorkflowService> logger
, ILoggerFactory loggerFactory            )
        {
            _uow = uow;
            _tenantService = tenantService;
            _notificationService = notificationService;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        // ═══════════════════════════════════════
        //  DEFINITIONS
        // ═══════════════════════════════════════

        public async Task<WorkflowDefinitionDetailVm> GetDefinitionByIdAsync(int id)
        {
            var def = await _uow.WorkflowInstances.GetDefinitionWithStepsAsync(id);
            if (def == null) throw new KeyNotFoundException("Workflow definition not found.");

            var instanceCount = await _uow.WorkflowInstances.Query()
                .CountAsync(i => i.WorkflowDefinitionId == id);

            return MapDefinition(def, instanceCount);
        }

        public async Task<IReadOnlyList<WorkflowDefinitionListVm>> GetDefinitionsAsync()
        {
            var defs = await _uow.WorkflowDefinitions.Query()
                .Include(d => d.Steps).Include(d => d.Instances)
                .OrderBy(d => d.Category).ThenBy(d => d.Name)
                .ToListAsync();

            return defs.Select(d => new WorkflowDefinitionListVm
            {
                Id = d.Id,
                Name = d.Name,
                Category = d.Category,
                Status = d.Status,
                Version = d.Version,
                StepCount = d.Steps.Count,
                InstanceCount = d.Instances.Count
            }).ToList();
        }

        public async Task<WorkflowDefinitionDetailVm> CreateDefinitionAsync(WorkflowDefinitionCreateVm model)
        {
            var def = new WorkflowDefinition
            {
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                Module = model.Module,
                AutoStart = model.AutoStart,
                Status = WorkflowStatus.Draft,
                Version = "1.0"
            };
            await _uow.WorkflowDefinitions.AddAsync(def);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Workflow definition created: {Name}", def.Name);
            return await GetDefinitionByIdAsync(def.Id);
        }

        public async Task<WorkflowDefinitionDetailVm> AddStepAsync(WorkflowStepCreateVm model)
        {
            var def = await _uow.WorkflowDefinitions.GetByIdAsync(model.WorkflowDefinitionId);
            if (def == null) throw new KeyNotFoundException("Workflow definition not found.");
            if (def.Status == WorkflowStatus.Active)
                throw new InvalidOperationException("Cannot modify an active workflow. Deactivate first.");

            var step = new WorkflowStep
            {
                WorkflowDefinitionId = model.WorkflowDefinitionId,
                StepOrder = model.StepOrder,
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                ExecutionMode = model.ExecutionMode,
                AssigneeRole = model.AssigneeRole,
                EscalationHours = model.EscalationHours,
                EscalationToRole = model.EscalationToRole,
                NotificationChannel = model.NotificationChannel
            };
            await _uow.WorkflowSteps.AddAsync(step);
            await _uow.SaveChangesAsync();

            return await GetDefinitionByIdAsync(model.WorkflowDefinitionId);
        }

        public async Task RemoveStepAsync(int stepId)
        {
            var step = await _uow.WorkflowSteps.GetByIdAsync(stepId);
            if (step == null) throw new KeyNotFoundException("Step not found.");
            _uow.WorkflowSteps.SoftDelete(step);
            await _uow.SaveChangesAsync();
        }

        public async Task<WorkflowDefinitionDetailVm> ActivateDefinitionAsync(int id)
        {
            var def = await _uow.WorkflowInstances.GetDefinitionWithStepsAsync(id);
            if (def == null) throw new KeyNotFoundException("Workflow definition not found.");
            if (!def.Steps.Any()) throw new InvalidOperationException("Cannot activate a workflow with no steps.");

            def.Status = WorkflowStatus.Active;
            _uow.WorkflowDefinitions.Update(def);
            await _uow.SaveChangesAsync();
            return await GetDefinitionByIdAsync(id);
        }

        public async Task<WorkflowDefinitionDetailVm> DeactivateDefinitionAsync(int id)
        {
            var def = await _uow.WorkflowDefinitions.GetByIdAsync(id);
            if (def == null) throw new KeyNotFoundException("Workflow definition not found.");
            def.Status = WorkflowStatus.Inactive;
            _uow.WorkflowDefinitions.Update(def);
            await _uow.SaveChangesAsync();
            return await GetDefinitionByIdAsync(id);
        }

        // ═══════════════════════════════════════
        //  INSTANCES
        // ═══════════════════════════════════════

        public async Task<WorkflowInstanceDetailVm> GetInstanceByIdAsync(int id)
        {
            var inst = await _uow.WorkflowInstances.GetWithDetailsAsync(id);
            if (inst == null) throw new KeyNotFoundException("Workflow instance not found.");
            return MapInstance(inst);
        }

        public async Task<WorkflowListResultVm> GetInstancesAsync(WorkflowFilterVm filter)
        {
            var query = _uow.WorkflowInstances.Query()
                .Include(i => i.WorkflowDefinition).ThenInclude(d => d.Steps)
                .AsQueryable();

            if (filter.Status.HasValue) query = query.Where(i => i.Status == filter.Status.Value);
            if (filter.Category.HasValue) query = query.Where(i => i.WorkflowDefinition.Category == filter.Category.Value);
            if (!string.IsNullOrEmpty(filter.EntityType)) query = query.Where(i => i.EntityType == filter.EntityType);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(i => i.CreatedDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            return new WorkflowListResultVm
            {
                Items = items.Select(i => new WorkflowInstanceListVm
                {
                    Id = i.Id,
                    InstanceNumber = i.InstanceNumber,
                    WorkflowName = i.WorkflowDefinition.Name,
                    Category = i.WorkflowDefinition.Category,
                    Status = i.Status,
                    EntityType = i.EntityType,
                    EntityReference = i.EntityReference,
                    CurrentStepOrder = i.CurrentStepOrder,
                    TotalSteps = i.WorkflowDefinition.Steps.Count,
                    CreatedDate = i.CreatedDate
                }).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<WorkflowInstanceDetailVm> StartWorkflowAsync(StartWorkflowVm model)
        {
            var def = await _uow.WorkflowInstances.GetDefinitionWithStepsAsync(model.WorkflowDefinitionId);
            if (def == null) throw new KeyNotFoundException("Workflow definition not found.");
            if (def.Status != WorkflowStatus.Active)
                throw new InvalidOperationException("Workflow definition is not active.");
            if (!def.Steps.Any())
                throw new InvalidOperationException("Workflow has no steps defined.");

            var number = await WorkflowNumberGenerator.GenerateAsync(_uow);

            var instance = new WorkflowInstance
            {
                InstanceNumber = number,
                WorkflowDefinitionId = model.WorkflowDefinitionId,
                Status = WorkflowInstanceStatus.InProgress,
                CurrentStepOrder = 1,
                EntityType = model.EntityType,
                EntityId = model.EntityId,
                EntityReference = model.EntityReference,
                InitiatedByUserId = _tenantService.GetCurrentUserId(),
                Notes = model.Notes
            };

            await _uow.WorkflowInstances.AddAsync(instance);
            await _uow.SaveChangesAsync();

            // Notify first step assignee
            var firstStep = def.Steps.OrderBy(s => s.StepOrder).First();
            await NotifyStepAssigneeAsync(instance, firstStep);

            _logger.LogInformation("Workflow started: {Number} ({Workflow}) for {EntityType}:{EntityId}",
                number, def.Name, model.EntityType, model.EntityId);

            return await GetInstanceByIdAsync(instance.Id);
        }

        public async Task<WorkflowInstanceDetailVm> StartWorkflowByCategoryAsync(
            WorkflowCategory category, string entityType, int entityId, string? entityReference)
        {
            var def = await _uow.WorkflowInstances.GetActiveDefinitionByCategoryAsync(category);
            if (def == null) return null!; // No workflow configured — silently skip

            return await StartWorkflowAsync(new StartWorkflowVm
            {
                WorkflowDefinitionId = def.Id,
                EntityType = entityType,
                EntityId = entityId,
                EntityReference = entityReference
            });
        }

        public async Task<WorkflowInstanceDetailVm> SubmitActionAsync(WorkflowActionSubmitVm model)
        {
            var instance = await _uow.WorkflowInstances.GetWithDetailsAsync(model.WorkflowInstanceId);
            if (instance == null) throw new KeyNotFoundException("Workflow instance not found.");

            if (instance.Status != WorkflowInstanceStatus.InProgress)
                throw new InvalidOperationException("Workflow is not in progress.");

            var currentStep = instance.WorkflowDefinition.Steps
                .FirstOrDefault(s => s.StepOrder == instance.CurrentStepOrder);
            if (currentStep == null) throw new InvalidOperationException("Current step not found.");

            var userId = _tenantService.GetCurrentUserId();

            // Record action
            var action = new WorkflowAction
            {
                WorkflowInstanceId = instance.Id,
                WorkflowStepId = currentStep.Id,
                StepOrder = instance.CurrentStepOrder,
                Result = model.Action,
                ActionByUserId = userId,
                ActionDate = DateTimeOffset.UtcNow,
                Comments = model.Comments,
                DelegatedToUserId = model.DelegateToUserId,
                DelegatedToUserName = model.DelegateToUserName
            };
            await _uow.WorkflowActions.AddAsync(action);

            // Process result
            switch (model.Action)
            {
                case StepActionResult.Approved:
                    await AdvanceToNextStepAsync(instance);
                    break;

                case StepActionResult.Rejected:
                    instance.Status = WorkflowInstanceStatus.Rejected;
                    instance.CompletedDate = DateTimeOffset.UtcNow;

                    // Notify initiator
                    await _notificationService.SendAsync(
                        instance.InitiatedByUserId,
                        $"Workflow Rejected: {instance.InstanceNumber}",
                        $"Your workflow for {instance.EntityReference ?? instance.EntityType} was rejected at step '{currentStep.Name}'. Comments: {model.Comments}",
                        $"/Workflow/Instance/{instance.Id}",
                        NotificationChannel.Both, instance.Id);
                    break;

                case StepActionResult.Delegated:
                    if (!model.DelegateToUserId.HasValue)
                        throw new InvalidOperationException("Delegation target user is required.");

                    await _notificationService.SendAsync(
                        model.DelegateToUserId.Value,
                        $"Workflow Delegated: {instance.InstanceNumber}",
                        $"Step '{currentStep.Name}' has been delegated to you for {instance.EntityReference ?? instance.EntityType}.",
                        $"/Workflow/Instance/{instance.Id}",
                        NotificationChannel.Both, instance.Id);
                    break;

                case StepActionResult.Escalated:
                    if (!string.IsNullOrEmpty(currentStep.EscalationToRole))
                    {
                        _logger.LogWarning("Workflow {Number} step {Step} escalated to role {Role}",
                            instance.InstanceNumber, currentStep.Name, currentStep.EscalationToRole);
                    }
                    break;
            }

            _uow.WorkflowInstances.Update(instance);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Workflow {Number} step {Step}: {Result} by user {User}",
                instance.InstanceNumber, currentStep.Name, model.Action, userId);

            return await GetInstanceByIdAsync(instance.Id);
        }

        public async Task<WorkflowInstanceDetailVm> CancelInstanceAsync(int id)
        {
            var instance = await _uow.WorkflowInstances.GetByIdAsync(id);
            if (instance == null) throw new KeyNotFoundException("Workflow instance not found.");

            instance.Status = WorkflowInstanceStatus.Cancelled;
            instance.CompletedDate = DateTimeOffset.UtcNow;
            _uow.WorkflowInstances.Update(instance);
            await _uow.SaveChangesAsync();

            return await GetInstanceByIdAsync(id);
        }

        public async Task<IReadOnlyList<WorkflowInstanceListVm>> GetPendingApprovalsAsync()
        {
            var instances = await _uow.WorkflowInstances.Query()
                .Include(i => i.WorkflowDefinition).ThenInclude(d => d.Steps)
                .Where(i => i.Status == WorkflowInstanceStatus.InProgress)
                .OrderBy(i => i.CreatedDate)
                .ToListAsync();

            return instances.Select(i => new WorkflowInstanceListVm
            {
                Id = i.Id,
                InstanceNumber = i.InstanceNumber,
                WorkflowName = i.WorkflowDefinition.Name,
                Category = i.WorkflowDefinition.Category,
                Status = i.Status,
                EntityType = i.EntityType,
                EntityReference = i.EntityReference,
                CurrentStepOrder = i.CurrentStepOrder,
                TotalSteps = i.WorkflowDefinition.Steps.Count,
                CreatedDate = i.CreatedDate
            }).ToList();
        }

        // ═══════════════════════════════════════
        //  INTERNAL
        // ═══════════════════════════════════════

        private async Task AdvanceToNextStepAsync(WorkflowInstance instance)
        {
            var allSteps = instance.WorkflowDefinition.Steps.OrderBy(s => s.StepOrder).ToList();
            var nextStep = allSteps.FirstOrDefault(s => s.StepOrder > instance.CurrentStepOrder);

            if (nextStep != null)
            {
                instance.CurrentStepOrder = nextStep.StepOrder;
                await NotifyStepAssigneeAsync(instance, nextStep);
            }
            else
            {
                // Workflow complete
                instance.Status = WorkflowInstanceStatus.Completed;
                instance.CompletedDate = DateTimeOffset.UtcNow;

                // Notify initiator
                await _notificationService.SendAsync(
                    instance.InitiatedByUserId,
                    $"Workflow Completed: {instance.InstanceNumber}",
                    $"Your workflow for {instance.EntityReference ?? instance.EntityType} has been fully approved.",
                    $"/Workflow/Instance/{instance.Id}",
                    NotificationChannel.Both, instance.Id);

                _logger.LogInformation("Workflow {Number} completed — all steps approved", instance.InstanceNumber);
            }
        }

        private async Task NotifyStepAssigneeAsync(WorkflowInstance instance, WorkflowStep step)
        {
            if (step.AssigneeUserId.HasValue)
            {
                await _notificationService.SendAsync(
                    step.AssigneeUserId.Value,
                    $"Action Required: {instance.InstanceNumber}",
                    $"Step '{step.Name}' requires your {step.Type} for {instance.EntityReference ?? instance.EntityType}.",
                    $"/Workflow/Instance/{instance.Id}",
                    step.NotificationChannel, instance.Id);
            }
            else
            {
                _logger.LogInformation("Workflow {Number} step '{Step}' awaiting action from role '{Role}'",
                    instance.InstanceNumber, step.Name, step.AssigneeRole);
            }
        }

        // ═══════════════════════════════════════
        //  MAPPING
        // ═══════════════════════════════════════

        private static WorkflowDefinitionDetailVm MapDefinition(WorkflowDefinition def, int instanceCount) => new()
        {
            Id = def.Id,
            Name = def.Name,
            Description = def.Description,
            Category = def.Category,
            Status = def.Status,
            Version = def.Version,
            Module = def.Module,
            AutoStart = def.AutoStart,
            InstanceCount = instanceCount,
            Steps = def.Steps.OrderBy(s => s.StepOrder).Select(s => new WorkflowStepVm
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                ExecutionMode = s.ExecutionMode,
                AssigneeRole = s.AssigneeRole,
                AssigneeUserId = s.AssigneeUserId,
                EscalationHours = s.EscalationHours,
                EscalationToRole = s.EscalationToRole,
                NotificationChannel = s.NotificationChannel
            }).ToList()
        };

        private static WorkflowInstanceDetailVm MapInstance(WorkflowInstance inst)
        {
            var allSteps = inst.WorkflowDefinition.Steps.OrderBy(s => s.StepOrder).ToList();
            var currentStep = allSteps.FirstOrDefault(s => s.StepOrder == inst.CurrentStepOrder);

            return new WorkflowInstanceDetailVm
            {
                Id = inst.Id,
                InstanceNumber = inst.InstanceNumber,
                WorkflowName = inst.WorkflowDefinition.Name,
                Category = inst.WorkflowDefinition.Category,
                Status = inst.Status,
                CurrentStepOrder = inst.CurrentStepOrder,
                TotalSteps = allSteps.Count,
                EntityType = inst.EntityType,
                EntityId = inst.EntityId,
                EntityReference = inst.EntityReference,
                CreatedDate = inst.CreatedDate,
                CompletedDate = inst.CompletedDate,
                Notes = inst.Notes,
                CurrentStep = currentStep != null ? new WorkflowStepVm
                {
                    Id = currentStep.Id,
                    StepOrder = currentStep.StepOrder,
                    Name = currentStep.Name,
                    Description = currentStep.Description,
                    Type = currentStep.Type,
                    ExecutionMode = currentStep.ExecutionMode,
                    AssigneeRole = currentStep.AssigneeRole,
                    EscalationHours = currentStep.EscalationHours,
                    NotificationChannel = currentStep.NotificationChannel
                } : null,
                AllSteps = allSteps.Select(s => new WorkflowStepVm
                {
                    Id = s.Id,
                    StepOrder = s.StepOrder,
                    Name = s.Name,
                    Description = s.Description,
                    Type = s.Type,
                    ExecutionMode = s.ExecutionMode,
                    AssigneeRole = s.AssigneeRole,
                    EscalationHours = s.EscalationHours,
                    NotificationChannel = s.NotificationChannel
                }).ToList(),
                Actions = inst.Actions.OrderBy(a => a.ActionDate).Select(a => new WorkflowActionVm
                {
                    Id = a.Id,
                    StepOrder = a.StepOrder,
                    StepName = a.WorkflowStep?.Name ?? $"Step {a.StepOrder}",
                    Result = a.Result,
                    ActionByUserName = a.ActionByUserName,
                    ActionDate = a.ActionDate,
                    Comments = a.Comments,
                    DelegatedToUserName = a.DelegatedToUserName,
                    HasSignature = !string.IsNullOrEmpty(a.SignatureData)
                }).ToList()
            };
        }

        public async Task<EscalationResultVm> ProcessEscalationsAsync()
        {
            var engine = new EscalationEngine(_uow, _notificationService,
                _loggerFactory.CreateLogger<EscalationEngine>());
            return await engine.ProcessEscalationsAsync();
        }

        public async Task<int> SeedTemplatesAsync()
        {
            var seeder = new WorkflowTemplateSeeder(_uow,
                _loggerFactory.CreateLogger<WorkflowTemplateSeeder>());
            return await seeder.SeedTemplatesAsync();
        }
    }
}
