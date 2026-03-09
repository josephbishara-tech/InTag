using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Workflow
{
    /// <summary>
    /// Template defining a workflow with ordered steps.
    /// </summary>
    public class WorkflowDefinition : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public WorkflowCategory Category { get; set; }

        [Required]
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

        [Required, MaxLength(20)]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Module this workflow applies to (e.g. "Asset", "Document")
        /// </summary>
        [MaxLength(50)]
        public string? Module { get; set; }

        public bool AutoStart { get; set; }

        // Navigation
        public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
    }
}
