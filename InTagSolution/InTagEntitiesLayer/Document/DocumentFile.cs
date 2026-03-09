using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Document
{
    public class DocumentFile : BaseEntity
    {
        [Required]
        public int RevisionId { get; set; }

        [Required, MaxLength(300)]
        public string FileName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string FileType { get; set; } = null!;

        public long FileSize { get; set; }

        /// <summary>
        /// Azure Blob Storage path or local path
        /// </summary>
        [Required, MaxLength(1000)]
        public string StoragePath { get; set; } = null!;

        /// <summary>
        /// SHA-256 hash for integrity verification
        /// </summary>
        [MaxLength(128)]
        public string? Hash { get; set; }

        /// <summary>
        /// Extracted text content for full-text search indexing
        /// </summary>
        public string? ContentIndex { get; set; }

        // Navigation
        public DocumentRevision Revision { get; set; } = null!;
    }
}
