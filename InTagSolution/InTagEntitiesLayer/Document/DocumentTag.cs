using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Document
{
    /// <summary>
    /// Tag attached to a document, file, or folder.
    /// Supports both controlled documents and user files.
    /// </summary>
    public class DocumentTag : BaseEntity
    {
        public int? DocumentId { get; set; }
        public int? UserFileId { get; set; }
        public int? UserFolderId { get; set; }

        [Required, MaxLength(100)]
        public string Tag { get; set; } = null!;

        [MaxLength(50)]
        public string? Color { get; set; }

        // Navigation
        public Document? Document { get; set; }
        public UserFile? UserFile { get; set; }
        public UserFolder? UserFolder { get; set; }
    }
}
