using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    /// <summary>
    /// Defines a metadata field (like a column definition).
    /// Can belong to a template or be standalone (tenant-wide).
    /// </summary>
    public class MetadataFieldDefinition : BaseEntity
    {
        [Required, MaxLength(100)]
        public string FieldName { get; set; } = null!;

        [MaxLength(300)]
        public string? DisplayLabel { get; set; }

        [Required]
        public MetadataFieldType FieldType { get; set; }

        /// <summary>
        /// For dropdowns: pipe-delimited options e.g. "Option1|Option2|Option3"
        /// </summary>
        [MaxLength(2000)]
        public string? Options { get; set; }

        public bool IsRequired { get; set; }

        [MaxLength(500)]
        public string? DefaultValue { get; set; }

        [MaxLength(200)]
        public string? Placeholder { get; set; }

        /// <summary>
        /// Display order within a template or document
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Template this field belongs to (null = standalone field)
        /// </summary>
        public int? TemplateId { get; set; }

        /// <summary>
        /// Is this a system-managed field (non-deletable)?
        /// </summary>
        public bool IsSystem { get; set; }

        [MaxLength(500)]
        public string? HelpText { get; set; }

        // Navigation
        public MetadataTemplate? Template { get; set; }
    }
}
