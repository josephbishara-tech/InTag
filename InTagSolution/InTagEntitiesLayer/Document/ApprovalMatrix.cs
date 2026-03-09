using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    public class ApprovalMatrix : BaseEntity
    {
        [Required]
        public DocumentType DocumentType { get; set; }

        public int? DepartmentId { get; set; }

        /// <summary>
        /// Approval level in the chain (1 = first approver, 2 = second, etc.)
        /// </summary>
        [Required]
        public int ApproverLevel { get; set; }

        /// <summary>
        /// Role required at this level (e.g. "Manager", "QualityLead")
        /// </summary>
        [Required, MaxLength(100)]
        public string ApproverRole { get; set; } = null!;

        /// <summary>
        /// Specific user ID if a named approver is required
        /// </summary>
        public Guid? ApproverUserId { get; set; }

        /// <summary>
        /// Auto-escalate after this many hours if not actioned
        /// </summary>
        public int? EscalationHours { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public Asset.Department? Department { get; set; }
    }
}
