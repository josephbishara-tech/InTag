using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    /// <summary>
    /// Reusable metadata template that defines a set of fields.
    /// Users can apply templates to documents to auto-populate metadata fields.
    /// </summary>
    public class MetadataTemplate : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Which document types this template applies to (null = all)
        /// </summary>
        public DocumentType? ApplicableDocType { get; set; }

        public bool IsDefault { get; set; }

        // Navigation
        public ICollection<MetadataFieldDefinition> Fields { get; set; } = new List<MetadataFieldDefinition>();
    }
}
