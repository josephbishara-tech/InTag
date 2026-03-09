using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Workflow
{
    /// <summary>
    /// Running instance of a workflow linked to a specific entity.
    /// </summary>
    public class WorkflowInstance : BaseEntity
    {
        [Required, MaxLength(50)]
        public string InstanceNumber { get; set; } = null!;

        [Required]
        public int WorkflowDefinitionId { get; set; }

        [Required]
        public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Pending;

        /// <summary>
        /// Currently active step order
        /// </summary>
        public int CurrentStepOrder { get; set; } = 1;

        /// <summary>
        /// Type of entity this instance is linked to (e.g. "Asset", "Document", "WorkOrder")
        /// </summary>
        [Required, MaxLength(100)]
        public string EntityType { get; set; } = null!;

        /// <summary>
        /// ID of the linked entity
        /// </summary>
        [Required]
        public int EntityId { get; set; }

        /// <summary>
        /// Display reference (e.g. "AST-00042", "DOC-QA-00005")
        /// </summary>
        [MaxLength(200)]
        public string? EntityReference { get; set; }

        public Guid InitiatedByUserId { get; set; }

        public DateTimeOffset? CompletedDate { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
        public ICollection<WorkflowAction> Actions { get; set; } = new List<WorkflowAction>();
    }
}
