using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Workflow
{
    public class WorkflowStep : BaseEntity
    {
        [Required]
        public int WorkflowDefinitionId { get; set; }

        [Required]
        public int StepOrder { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public StepType Type { get; set; } = StepType.Approval;

        [Required]
        public StepExecutionMode ExecutionMode { get; set; } = StepExecutionMode.Sequential;

        /// <summary>
        /// Role required (e.g. "Manager", "QualityLead", "Admin")
        /// </summary>
        [Required, MaxLength(100)]
        public string AssigneeRole { get; set; } = null!;

        /// <summary>
        /// Specific user — if null, any user with AssigneeRole can act
        /// </summary>
        public Guid? AssigneeUserId { get; set; }

        public int? DepartmentId { get; set; }

        /// <summary>
        /// Auto-escalate if not actioned within this many hours
        /// </summary>
        public int? EscalationHours { get; set; }

        [MaxLength(100)]
        public string? EscalationToRole { get; set; }

        /// <summary>
        /// Condition expression for conditional branching (JSON)
        /// </summary>
        [MaxLength(2000)]
        public string? ConditionExpression { get; set; }

        [Required]
        public NotificationChannel NotificationChannel { get; set; } = NotificationChannel.Both;

        // Navigation
        public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
        public Asset.Department? Department { get; set; }
    }
}
