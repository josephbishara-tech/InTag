using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    public class DistributionRecord : BaseEntity
    {
        [Required]
        public int DocumentId { get; set; }

        [Required]
        public DistributionMethod Method { get; set; }

        /// <summary>
        /// "User" or "Department" or "External"
        /// </summary>
        [Required, MaxLength(50)]
        public string RecipientType { get; set; } = null!;

        /// <summary>
        /// User ID, Department ID, or email address depending on RecipientType
        /// </summary>
        [Required, MaxLength(200)]
        public string RecipientIdentifier { get; set; } = null!;

        [MaxLength(200)]
        public string? RecipientName { get; set; }

        [Required]
        public DateTimeOffset SentDate { get; set; }

        public DateTimeOffset? AcknowledgedDate { get; set; }

        public bool IsAcknowledged => AcknowledgedDate.HasValue;

        // Navigation
        public Document Document { get; set; } = null!;
    }
}
