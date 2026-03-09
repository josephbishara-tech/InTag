using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Workflow
{
    /// <summary>
    /// Records each action taken on a workflow step — full audit trail.
    /// </summary>
    public class WorkflowAction : BaseEntity
    {
        [Required]
        public int WorkflowInstanceId { get; set; }

        [Required]
        public int WorkflowStepId { get; set; }

        public int StepOrder { get; set; }

        [Required]
        public StepActionResult Result { get; set; }

        public Guid ActionByUserId { get; set; }

        [MaxLength(200)]
        public string? ActionByUserName { get; set; }

        [Required]
        public DateTimeOffset ActionDate { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        /// <summary>
        /// If delegated, the user it was delegated to
        /// </summary>
        public Guid? DelegatedToUserId { get; set; }

        [MaxLength(200)]
        public string? DelegatedToUserName { get; set; }

        /// <summary>
        /// Digital signature data for compliance
        /// </summary>
        [MaxLength(500)]
        public string? SignatureData { get; set; }

        // Navigation
        public WorkflowInstance WorkflowInstance { get; set; } = null!;
        public WorkflowStep WorkflowStep { get; set; } = null!;
    }
}
