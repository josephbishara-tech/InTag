using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Workflow
{
    public class WorkflowInstanceDetailVm
    {
        public int Id { get; set; }
        public string InstanceNumber { get; set; } = null!;
        public string WorkflowName { get; set; } = null!;
        public WorkflowCategory Category { get; set; }
        public string CategoryDisplay => Category.ToString();
        public WorkflowInstanceStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public int CurrentStepOrder { get; set; }
        public int TotalSteps { get; set; }
        public string EntityType { get; set; } = null!;
        public int EntityId { get; set; }
        public string? EntityReference { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public string? Notes { get; set; }

        public WorkflowStepVm? CurrentStep { get; set; }
        public IReadOnlyList<WorkflowStepVm> AllSteps { get; set; } = new List<WorkflowStepVm>();
        public IReadOnlyList<WorkflowActionVm> Actions { get; set; } = new List<WorkflowActionVm>();
    }

    public class WorkflowInstanceListVm
    {
        public int Id { get; set; }
        public string InstanceNumber { get; set; } = null!;
        public string WorkflowName { get; set; } = null!;
        public WorkflowCategory Category { get; set; }
        public string CategoryDisplay => Category.ToString();
        public WorkflowInstanceStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string EntityType { get; set; } = null!;
        public string? EntityReference { get; set; }
        public int CurrentStepOrder { get; set; }
        public int TotalSteps { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class WorkflowActionVm
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = null!;
        public StepActionResult Result { get; set; }
        public string ResultDisplay => Result.ToString();
        public string? ActionByUserName { get; set; }
        public DateTimeOffset ActionDate { get; set; }
        public string? Comments { get; set; }
        public string? DelegatedToUserName { get; set; }
        public bool HasSignature { get; set; }
    }

    public class StartWorkflowVm
    {
        [Required]
        public int WorkflowDefinitionId { get; set; }

        [Required, MaxLength(100)]
        public string EntityType { get; set; } = null!;

        [Required]
        public int EntityId { get; set; }

        [MaxLength(200)]
        public string? EntityReference { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class WorkflowActionSubmitVm
    {
        [Required]
        public int WorkflowInstanceId { get; set; }

        [Required]
        public StepActionResult Action { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public Guid? DelegateToUserId { get; set; }

        [MaxLength(200)]
        public string? DelegateToUserName { get; set; }
    }

    public class WorkflowFilterVm
    {
        public WorkflowInstanceStatus? Status { get; set; }
        public WorkflowCategory? Category { get; set; }
        public string? EntityType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkflowListResultVm
    {
        public IReadOnlyList<WorkflowInstanceListVm> Items { get; set; } = new List<WorkflowInstanceListVm>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
