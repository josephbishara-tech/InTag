using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Common;

namespace InTagLogicLayer.Workflow
{
    /// <summary>
    /// Processes overdue workflow steps and auto-escalates based on configured hours.
    /// Designed to be called periodically (e.g. Hangfire scheduled job).
    /// </summary>
    public class EscalationEngine
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifService;
        private readonly ILogger<EscalationEngine> _logger;

        public EscalationEngine(IUnitOfWork uow, INotificationService notifService,
            ILogger<EscalationEngine> logger)
        {
            _uow = uow;
            _notifService = notifService;
            _logger = logger;
        }

        public async Task<EscalationResultVm> ProcessEscalationsAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var results = new List<EscalationItemVm>();

            var activeInstances = await _uow.WorkflowInstances.Query()
                .Include(i => i.WorkflowDefinition).ThenInclude(d => d.Steps)
                .Include(i => i.Actions)
                .Where(i => i.Status == WorkflowInstanceStatus.InProgress)
                .ToListAsync();

            foreach (var instance in activeInstances)
            {
                var currentStep = instance.WorkflowDefinition.Steps
                    .FirstOrDefault(s => s.StepOrder == instance.CurrentStepOrder);

                if (currentStep == null || !currentStep.EscalationHours.HasValue) continue;

                // Find when the current step became active
                var lastAction = instance.Actions
                    .Where(a => a.StepOrder == instance.CurrentStepOrder - 1)
                    .OrderByDescending(a => a.ActionDate)
                    .FirstOrDefault();

                var stepActivesSince = lastAction?.ActionDate ?? instance.CreatedDate;
                var hoursElapsed = (decimal)(now - stepActivesSince).TotalHours;

                if (hoursElapsed < currentStep.EscalationHours.Value) continue;

                // Check if already escalated for this step
                var alreadyEscalated = instance.Actions
                    .Any(a => a.StepOrder == instance.CurrentStepOrder
                              && a.Result == StepActionResult.Escalated);

                if (alreadyEscalated) continue;

                // Record escalation action
                var escalationAction = new WorkflowAction
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepId = currentStep.Id,
                    StepOrder = instance.CurrentStepOrder,
                    Result = StepActionResult.Escalated,
                    ActionByUserId = Guid.Empty,
                    ActionByUserName = "System (Auto-Escalation)",
                    ActionDate = now,
                    Comments = $"Auto-escalated after {currentStep.EscalationHours}h. Elapsed: {hoursElapsed:N1}h."
                };
                await _uow.WorkflowActions.AddAsync(escalationAction);

                // Send escalation notification
                var escalationRole = currentStep.EscalationToRole ?? "Admin";
                await _notifService.SendAsync(
                    Guid.Empty, // In production: resolve user by role
                    $"ESCALATION: {instance.InstanceNumber}",
                    $"Workflow step '{currentStep.Name}' has exceeded the {currentStep.EscalationHours}h SLA. " +
                    $"Entity: {instance.EntityReference ?? instance.EntityType}. " +
                    $"Original assignee role: {currentStep.AssigneeRole}. Escalated to: {escalationRole}.",
                    $"/Workflow/Instance/{instance.Id}",
                    NotificationChannel.Both, instance.Id);

                // Also remind original assignee
                if (currentStep.AssigneeUserId.HasValue)
                {
                    await _notifService.SendAsync(
                        currentStep.AssigneeUserId.Value,
                        $"Overdue Reminder: {instance.InstanceNumber}",
                        $"Step '{currentStep.Name}' is overdue by {(hoursElapsed - currentStep.EscalationHours.Value):N1}h. Please action immediately.",
                        $"/Workflow/Instance/{instance.Id}",
                        NotificationChannel.Both, instance.Id);
                }

                results.Add(new EscalationItemVm
                {
                    InstanceNumber = instance.InstanceNumber,
                    WorkflowName = instance.WorkflowDefinition.Name,
                    StepName = currentStep.Name,
                    OriginalRole = currentStep.AssigneeRole,
                    EscalatedToRole = escalationRole,
                    HoursOverdue = Math.Round(hoursElapsed - currentStep.EscalationHours.Value, 1),
                    EntityReference = instance.EntityReference
                });

                _logger.LogWarning("Auto-escalation: {Instance} step '{Step}' — {Hours}h overdue, escalated to {Role}",
                    instance.InstanceNumber, currentStep.Name, hoursElapsed, escalationRole);
            }

            await _uow.SaveChangesAsync();

            return new EscalationResultVm
            {
                ProcessedCount = activeInstances.Count,
                EscalatedCount = results.Count,
                Items = results
            };
        }
    }

    public class EscalationResultVm
    {
        public int ProcessedCount { get; set; }
        public int EscalatedCount { get; set; }
        public IReadOnlyList<EscalationItemVm> Items { get; set; } = new List<EscalationItemVm>();
    }

    public class EscalationItemVm
    {
        public string InstanceNumber { get; set; } = null!;
        public string WorkflowName { get; set; } = null!;
        public string StepName { get; set; } = null!;
        public string OriginalRole { get; set; } = null!;
        public string EscalatedToRole { get; set; } = null!;
        public decimal HoursOverdue { get; set; }
        public string? EntityReference { get; set; }
    }
}
