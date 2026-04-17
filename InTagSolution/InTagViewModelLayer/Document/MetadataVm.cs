using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    // ── Templates ──

    public class MetadataTemplateListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DocumentType? ApplicableDocType { get; set; }
        public string ApplicableDocTypeDisplay => ApplicableDocType?.ToString() ?? "All Types";
        public bool IsDefault { get; set; }
        public int FieldCount { get; set; }
    }

    public class MetadataTemplateDetailVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DocumentType? ApplicableDocType { get; set; }
        public bool IsDefault { get; set; }
        public IReadOnlyList<FieldDefinitionVm> Fields { get; set; } = new List<FieldDefinitionVm>();
    }

    public class MetadataTemplateCreateVm
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Applicable Document Type")]
        public DocumentType? ApplicableDocType { get; set; }

        [Display(Name = "Set as Default")]
        public bool IsDefault { get; set; }
    }

    // ── Field Definitions ──

    public class FieldDefinitionVm
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = null!;
        public string? DisplayLabel { get; set; }
        public string Label => DisplayLabel ?? FieldName;
        public MetadataFieldType FieldType { get; set; }
        public string FieldTypeDisplay => FieldType.ToString();
        public string? Options { get; set; }
        public IReadOnlyList<string> OptionsList => string.IsNullOrEmpty(Options)
            ? new List<string>() : Options.Split('|').ToList();
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? Placeholder { get; set; }
        public int SortOrder { get; set; }
        public bool IsSystem { get; set; }
        public string? HelpText { get; set; }
    }

    public class FieldDefinitionCreateVm
    {
        [Required, MaxLength(100)]
        [Display(Name = "Field Name")]
        public string FieldName { get; set; } = null!;

        [MaxLength(300)]
        [Display(Name = "Display Label")]
        public string? DisplayLabel { get; set; }

        [Required]
        [Display(Name = "Field Type")]
        public MetadataFieldType FieldType { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Options (pipe-delimited)")]
        public string? Options { get; set; }

        public bool IsRequired { get; set; }

        [MaxLength(500)]
        [Display(Name = "Default Value")]
        public string? DefaultValue { get; set; }

        [MaxLength(200)]
        public string? Placeholder { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [MaxLength(500)]
        [Display(Name = "Help Text")]
        public string? HelpText { get; set; }

        public int? TemplateId { get; set; }
    }

    // ── Metadata Values ──

    public class MetadataValueVm
    {
        public int? MetadataId { get; set; }
        public int FieldDefinitionId { get; set; }
        public string FieldName { get; set; } = null!;
        public string Label { get; set; } = null!;
        public MetadataFieldType FieldType { get; set; }
        public string? Options { get; set; }
        public IReadOnlyList<string> OptionsList => string.IsNullOrEmpty(Options)
            ? new List<string>() : Options.Split('|').ToList();
        public bool IsRequired { get; set; }
        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public string? Value { get; set; }
    }

    public class MetadataEditVm
    {
        public int? DocumentId { get; set; }
        public int? UserFileId { get; set; }
        public int? UserFolderId { get; set; }
        public string EntityName { get; set; } = null!;
        public IList<MetadataValueVm> Fields { get; set; } = new List<MetadataValueVm>();
    }

    // ── Tags ──

    public class DocumentTagVm
    {
        public int Id { get; set; }
        public string Tag { get; set; } = null!;
        public string? Color { get; set; }
    }

    public class TagAddVm
    {
        public int? DocumentId { get; set; }
        public int? UserFileId { get; set; }
        public int? UserFolderId { get; set; }

        [Required, MaxLength(100)]
        public string Tag { get; set; } = null!;

        [MaxLength(50)]
        public string? Color { get; set; }
    }

    // ── Links ──

    public class DocumentLinkVm
    {
        public int Id { get; set; }
        public int TargetDocumentId { get; set; }
        public string TargetDocNumber { get; set; } = null!;
        public string TargetTitle { get; set; } = null!;
        public DocumentLinkType LinkType { get; set; }
        public string LinkTypeDisplay => LinkType.ToString();
        public string? Description { get; set; }
    }

    public class DocumentLinkCreateVm
    {
        [Required]
        public int SourceDocumentId { get; set; }

        [Required]
        [Display(Name = "Target Document")]
        public int TargetDocumentId { get; set; }

        [Required]
        [Display(Name = "Link Type")]
        public DocumentLinkType LinkType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
