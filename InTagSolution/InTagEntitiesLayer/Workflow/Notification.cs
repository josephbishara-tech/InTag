using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Workflow
{
    public class Notification : BaseEntity
    {
        [Required]
        public Guid RecipientUserId { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Message { get; set; }

        [Required]
        public NotificationChannel Channel { get; set; }

        /// <summary>
        /// Link to relevant entity/action
        /// </summary>
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTimeOffset? ReadDate { get; set; }

        public bool IsEmailSent { get; set; }

        public DateTimeOffset? EmailSentDate { get; set; }

        public int? WorkflowInstanceId { get; set; }

        // Navigation
        public WorkflowInstance? WorkflowInstance { get; set; }
    }
}
