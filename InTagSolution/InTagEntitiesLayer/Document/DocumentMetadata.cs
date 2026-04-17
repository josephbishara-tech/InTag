using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Document
{
    /// <summary>
    /// Actual metadata value for a document or user file.
    /// Each row is a key-value pair: FieldDefinitionId → Value.
    /// </summary>
    public class DocumentMetadata : BaseEntity
    {
        /// <summary>
        /// The controlled document (ISO documents)
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// The user file (personal repository)
        /// </summary>
        public int? UserFileId { get; set; }

        /// <summary>
        /// The user folder
        /// </summary>
        public int? UserFolderId { get; set; }

        [Required]
        public int FieldDefinitionId { get; set; }

        /// <summary>
        /// Stored value (all types serialized as string)
        /// </summary>
        [MaxLength(4000)]
        public string? Value { get; set; }

        // Navigation
        public Document? Document { get; set; }
        public UserFile? UserFile { get; set; }
        public UserFolder? UserFolder { get; set; }
        public MetadataFieldDefinition FieldDefinition { get; set; } = null!;
    }
}
