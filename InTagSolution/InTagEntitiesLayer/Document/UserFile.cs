using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Document
{
    public class UserFile : BaseEntity
    {
        [Required]
        public int FolderId { get; set; }

        [Required, MaxLength(300)]
        public string FileName { get; set; } = null!;

        [Required, MaxLength(50)]
        public string FileType { get; set; } = null!;

        public long FileSize { get; set; }

        [Required, MaxLength(1000)]
        public string StoragePath { get; set; } = null!;

        [MaxLength(128)]
        public string? Hash { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Uploader user ID
        /// </summary>
        public Guid UploadedByUserId { get; set; }

        public DateTimeOffset UploadedDate { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public UserFolder Folder { get; set; } = null!;
        public ICollection<FileShare> Shares { get; set; } = new List<FileShare>();
    }
}
