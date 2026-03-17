using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Common;
using Microsoft.Extensions.Logging;

namespace InTagLogicLayer.Workflow
{
    /// <summary>
    /// Seeds pre-built workflow templates for common business processes.
    /// </summary>
    public class WorkflowTemplateSeeder
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<WorkflowTemplateSeeder> _logger;

        public WorkflowTemplateSeeder(IUnitOfWork uow, ILogger<WorkflowTemplateSeeder> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<int> SeedTemplatesAsync()
        {
            var seeded = 0;

            seeded += await SeedIfNotExistsAsync(new WorkflowDefinition
            {
                Name = "Asset Disposal Approval",
                Description = "Two-level approval for asset decommission and disposal.",
                Category = WorkflowCategory.AssetDisposal,
                Module = "Asset",
                Version = "1.0",
                Status = WorkflowStatus.Active,
                AutoStart = true
            }, new[]
            {
                new WorkflowStep { StepOrder = 1, Name = "Department Manager Approval", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Manager",
                    EscalationHours = 48, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 2, Name = "Finance Review", Type = StepType.Review,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Finance",
                    EscalationHours = 72, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 3, Name = "Final Approval", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Admin",
                    NotificationChannel = NotificationChannel.Both }
            });

            seeded += await SeedIfNotExistsAsync(new WorkflowDefinition
            {
                Name = "Document Review & Approval",
                Description = "Standard ISO-compliant document approval with review and sign-off.",
                Category = WorkflowCategory.DocumentApproval,
                Module = "Document",
                Version = "1.0",
                Status = WorkflowStatus.Active,
                AutoStart = false
            }, new[]
            {
                new WorkflowStep { StepOrder = 1, Name = "Technical Review", Type = StepType.Review,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Operator",
                    EscalationHours = 24, EscalationToRole = "Manager",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 2, Name = "Manager Approval", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Manager",
                    EscalationHours = 48, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both }
            });

            seeded += await SeedIfNotExistsAsync(new WorkflowDefinition
            {
                Name = "Production Order Release",
                Description = "Approval required before releasing production orders to the shop floor.",
                Category = WorkflowCategory.ProductionRelease,
                Module = "Manufacturing",
                Version = "1.0",
                Status = WorkflowStatus.Active,
                AutoStart = true
            }, new[]
            {
                new WorkflowStep { StepOrder = 1, Name = "Production Planner Review", Type = StepType.Review,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Operator",
                    NotificationChannel = NotificationChannel.InApp },
                new WorkflowStep { StepOrder = 2, Name = "Production Manager Approval", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Manager",
                    EscalationHours = 24, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both }
            });

            seeded += await SeedIfNotExistsAsync(new WorkflowDefinition
            {
                Name = "Work Order Approval",
                Description = "Approval for high-cost or critical maintenance work orders.",
                Category = WorkflowCategory.WorkOrderApproval,
                Module = "Maintenance",
                Version = "1.0",
                Status = WorkflowStatus.Active,
                AutoStart = false
            }, new[]
            {
                new WorkflowStep { StepOrder = 1, Name = "Maintenance Supervisor", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Manager",
                    EscalationHours = 12, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both }
            });

            seeded += await SeedIfNotExistsAsync(new WorkflowDefinition
            {
                Name = "Purchase Requisition",
                Description = "Multi-level approval for purchase requisitions from reorder alerts.",
                Category = WorkflowCategory.PurchaseRequisition,
                Module = "Inventory",
                Version = "1.0",
                Status = WorkflowStatus.Active,
                AutoStart = false
            }, new[]
            {
                new WorkflowStep { StepOrder = 1, Name = "Warehouse Manager", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Manager",
                    EscalationHours = 24, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 2, Name = "Finance Approval", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Finance",
                    EscalationHours = 48, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 3, Name = "Notification", Type = StepType.Notification,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Operator",
                    NotificationChannel = NotificationChannel.Email }
            });

            seeded += await SeedIfNotExistsAsync(new WorkflowDefinition
            {
                Name = "Change Request",
                Description = "Generic change request workflow for process or system changes.",
                Category = WorkflowCategory.ChangeRequest,
                Version = "1.0",
                Status = WorkflowStatus.Active,
                AutoStart = false
            }, new[]
            {
                new WorkflowStep { StepOrder = 1, Name = "Impact Assessment", Type = StepType.Review,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Operator",
                    EscalationHours = 48, EscalationToRole = "Manager",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 2, Name = "Department Approval", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "Manager",
                    EscalationHours = 72, EscalationToRole = "Admin",
                    NotificationChannel = NotificationChannel.Both },
                new WorkflowStep { StepOrder = 3, Name = "QA Sign-Off", Type = StepType.Approval,
                    ExecutionMode = StepExecutionMode.Sequential, AssigneeRole = "QualityLead",
                    NotificationChannel = NotificationChannel.Both }
            });

            return seeded;
        }

        private async Task<int> SeedIfNotExistsAsync(WorkflowDefinition def, WorkflowStep[] steps)
        {
            var exists = await _uow.WorkflowDefinitions.ExistsAsync(
                d => d.Name == def.Name && d.Version == def.Version);
            if (exists) return 0;

            await _uow.WorkflowDefinitions.AddAsync(def);
            await _uow.SaveChangesAsync();

            foreach (var step in steps)
            {
                step.WorkflowDefinitionId = def.Id;
                await _uow.WorkflowSteps.AddAsync(step);
            }
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Workflow template seeded: {Name} with {Steps} steps", def.Name, steps.Length);
            return 1;
        }
    }
}
