using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    public class FileShare : BaseEntity
    {
        [Required]
        public int UserFileId { get; set; }

        /// <summary>
        /// Who shared
        /// </summary>
        public Guid SharedByUserId { get; set; }

        /// <summary>
        /// Share target type
        /// </summary>
        [Required]
        public ShareTargetType TargetType { get; set; }

        /// <summary>
        /// Target user ID (when sharing with a specific user)
        /// </summary>
        public Guid? TargetUserId { get; set; }

        /// <summary>
        /// Target department ID (when sharing with a department)
        /// </summary>
        public int? TargetDepartmentId { get; set; }

        /// <summary>
        /// Permission level
        /// </summary>
        [Required]
        public SharePermission Permission { get; set; } = SharePermission.View;

        [MaxLength(500)]
        public string? Message { get; set; }

        public DateTimeOffset SharedDate { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public UserFile UserFile { get; set; } = null!;
        public Asset.Department? TargetDepartment { get; set; }
    }
}
