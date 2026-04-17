using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    /// <summary>
    /// Link between documents (related, supersedes, references, etc.)
    /// </summary>
    public class DocumentLink : BaseEntity
    {
        [Required]
        public int SourceDocumentId { get; set; }

        [Required]
        public int TargetDocumentId { get; set; }

        [Required]
        public DocumentLinkType LinkType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public Document SourceDocument { get; set; } = null!;
        public Document TargetDocument { get; set; } = null!;
    }
}
