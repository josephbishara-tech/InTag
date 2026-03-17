using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Workflow
{
    public class WorkflowDefinitionCreateVm
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public WorkflowCategory Category { get; set; }

        [MaxLength(50)]
        public string? Module { get; set; }

        public bool AutoStart { get; set; }
    }

    public class WorkflowDefinitionDetailVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public WorkflowCategory Category { get; set; }
        public string CategoryDisplay => Category.ToString();
        public WorkflowStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string Version { get; set; } = null!;
        public string? Module { get; set; }
        public bool AutoStart { get; set; }
        public IReadOnlyList<WorkflowStepVm> Steps { get; set; } = new List<WorkflowStepVm>();
        public int InstanceCount { get; set; }
    }

    public class WorkflowDefinitionListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public WorkflowCategory Category { get; set; }
        public string CategoryDisplay => Category.ToString();
        public WorkflowStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string Version { get; set; } = null!;
        public int StepCount { get; set; }
        public int InstanceCount { get; set; }
    }

    public class WorkflowStepVm
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public StepType Type { get; set; }
        public string TypeDisplay => Type.ToString();
        public StepExecutionMode ExecutionMode { get; set; }
        public string ExecutionDisplay => ExecutionMode.ToString();
        public string AssigneeRole { get; set; } = null!;
        public Guid? AssigneeUserId { get; set; }
        public int? EscalationHours { get; set; }
        public string? EscalationToRole { get; set; }
        public NotificationChannel NotificationChannel { get; set; }
    }

    public class WorkflowStepCreateVm
    {
        [Required]
        public int WorkflowDefinitionId { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Step Order")]
        public int StepOrder { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public StepType Type { get; set; } = StepType.Approval;

        [Required]
        [Display(Name = "Execution")]
        public StepExecutionMode ExecutionMode { get; set; } = StepExecutionMode.Sequential;

        [Required, MaxLength(100)]
        [Display(Name = "Assignee Role")]
        public string AssigneeRole { get; set; } = null!;

        [Display(Name = "Escalation Hours")]
        public int? EscalationHours { get; set; }

        [MaxLength(100)]
        [Display(Name = "Escalate To")]
        public string? EscalationToRole { get; set; }

        [Required]
        public NotificationChannel NotificationChannel { get; set; } = NotificationChannel.Both;
    }
}
