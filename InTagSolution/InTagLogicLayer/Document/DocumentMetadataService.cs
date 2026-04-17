using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public class DocumentMetadataService : IDocumentMetadataService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<DocumentMetadataService> _logger;

        public DocumentMetadataService(IUnitOfWork uow, ILogger<DocumentMetadataService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        //  TEMPLATES
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<MetadataTemplateListVm>> GetTemplatesAsync()
        {
            var templates = await _uow.MetadataTemplates.Query()
                .Include(t => t.Fields)
                .OrderBy(t => t.Name).ToListAsync();

            return templates.Select(t => new MetadataTemplateListVm
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ApplicableDocType = t.ApplicableDocType,
                IsDefault = t.IsDefault,
                FieldCount = t.Fields.Count(f => f.IsActive)
            }).ToList();
        }

        public async Task<MetadataTemplateDetailVm> GetTemplateByIdAsync(int id)
        {
            var t = await _uow.MetadataTemplates.Query()
                .Include(t => t.Fields.Where(f => f.IsActive).OrderBy(f => f.SortOrder))
                .FirstOrDefaultAsync(t => t.Id == id);

            if (t == null) throw new KeyNotFoundException("Template not found.");

            return new MetadataTemplateDetailVm
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ApplicableDocType = t.ApplicableDocType,
                IsDefault = t.IsDefault,
                Fields = t.Fields.Select(MapField).ToList()
            };
        }

        public async Task<MetadataTemplateDetailVm> CreateTemplateAsync(MetadataTemplateCreateVm model)
        {
            if (await _uow.MetadataTemplates.ExistsAsync(t => t.Name == model.Name))
                throw new InvalidOperationException($"Template '{model.Name}' already exists.");

            if (model.IsDefault)
            {
                var existingDefaults = await _uow.MetadataTemplates
                    .FindAsync(t => t.IsDefault && t.ApplicableDocType == model.ApplicableDocType);
                foreach (var d in existingDefaults) { d.IsDefault = false; _uow.MetadataTemplates.Update(d); }
            }

            var template = new MetadataTemplate
            {
                Name = model.Name,
                Description = model.Description,
                ApplicableDocType = model.ApplicableDocType,
                IsDefault = model.IsDefault
            };

            await _uow.MetadataTemplates.AddAsync(template);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Metadata template created: {Name}", template.Name);
            return await GetTemplateByIdAsync(template.Id);
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var t = await _uow.MetadataTemplates.GetByIdAsync(id);
            if (t == null) throw new KeyNotFoundException("Template not found.");
            _uow.MetadataTemplates.SoftDelete(t);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  FIELD DEFINITIONS
        // ═══════════════════════════════════════

        public async Task<FieldDefinitionVm> AddFieldToTemplateAsync(FieldDefinitionCreateVm model)
        {
            if (model.TemplateId.HasValue)
            {
                var exists = await _uow.MetadataFieldDefinitions.ExistsAsync(
                    f => f.TemplateId == model.TemplateId && f.FieldName == model.FieldName);
                if (exists) throw new InvalidOperationException($"Field '{model.FieldName}' already exists in this template.");
            }

            var field = new MetadataFieldDefinition
            {
                FieldName = model.FieldName,
                DisplayLabel = model.DisplayLabel,
                FieldType = model.FieldType,
                Options = model.Options,
                IsRequired = model.IsRequired,
                DefaultValue = model.DefaultValue,
                Placeholder = model.Placeholder,
                SortOrder = model.SortOrder,
                HelpText = model.HelpText,
                TemplateId = model.TemplateId
            };

            await _uow.MetadataFieldDefinitions.AddAsync(field);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Metadata field added: {Name} to template {TemplateId}", field.FieldName, model.TemplateId);
            return MapField(field);
        }

        public async Task RemoveFieldAsync(int fieldId)
        {
            var f = await _uow.MetadataFieldDefinitions.GetByIdAsync(fieldId);
            if (f == null) throw new KeyNotFoundException("Field not found.");
            if (f.IsSystem) throw new InvalidOperationException("Cannot delete system fields.");
            _uow.MetadataFieldDefinitions.SoftDelete(f);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  METADATA VALUES
        // ═══════════════════════════════════════

        public async Task<MetadataEditVm> GetMetadataAsync(int? documentId, int? userFileId, int? userFolderId)
        {
            // Get all field definitions (system + from applicable templates)
            var systemFields = await _uow.MetadataFieldDefinitions.Query()
                .Where(f => f.TemplateId == null && f.IsSystem)
                .OrderBy(f => f.SortOrder).ToListAsync();

            // Get template fields for this entity type
            var templateFields = new List<MetadataFieldDefinition>();
            if (documentId.HasValue)
            {
                var doc = await _uow.Documents.GetByIdAsync(documentId.Value);
                if (doc != null)
                {
                    var templates = await _uow.MetadataTemplates.Query()
                        .Include(t => t.Fields.Where(f => f.IsActive))
                        .Where(t => t.IsDefault && (t.ApplicableDocType == null || t.ApplicableDocType == doc.Type))
                        .ToListAsync();
                    templateFields = templates.SelectMany(t => t.Fields).OrderBy(f => f.SortOrder).ToList();
                }
            }

            var allFields = systemFields.Concat(templateFields)
                .DistinctBy(f => f.Id).OrderBy(f => f.SortOrder).ToList();

            // Get existing values
            var existingValues = await _uow.DocumentMetadatas.Query()
                .Where(m =>
                    (documentId.HasValue && m.DocumentId == documentId) ||
                    (userFileId.HasValue && m.UserFileId == userFileId) ||
                    (userFolderId.HasValue && m.UserFolderId == userFolderId))
                .ToListAsync();

            var entityName = documentId.HasValue ? "Document"
                : userFileId.HasValue ? "File" : "Folder";

            return new MetadataEditVm
            {
                DocumentId = documentId,
                UserFileId = userFileId,
                UserFolderId = userFolderId,
                EntityName = entityName,
                Fields = allFields.Select(f =>
                {
                    var existing = existingValues.FirstOrDefault(v => v.FieldDefinitionId == f.Id);
                    return new MetadataValueVm
                    {
                        MetadataId = existing?.Id,
                        FieldDefinitionId = f.Id,
                        FieldName = f.FieldName,
                        Label = f.DisplayLabel ?? f.FieldName,
                        FieldType = f.FieldType,
                        Options = f.Options,
                        IsRequired = f.IsRequired,
                        Placeholder = f.Placeholder,
                        HelpText = f.HelpText,
                        Value = existing?.Value ?? f.DefaultValue
                    };
                }).ToList()
            };
        }

        public async Task SaveMetadataAsync(MetadataEditVm model)
        {
            foreach (var field in model.Fields)
            {
                if (field.MetadataId.HasValue)
                {
                    var existing = await _uow.DocumentMetadatas.GetByIdAsync(field.MetadataId.Value);
                    if (existing != null)
                    {
                        existing.Value = field.Value;
                        _uow.DocumentMetadatas.Update(existing);
                    }
                }
                else if (!string.IsNullOrEmpty(field.Value))
                {
                    var meta = new DocumentMetadata
                    {
                        DocumentId = model.DocumentId,
                        UserFileId = model.UserFileId,
                        UserFolderId = model.UserFolderId,
                        FieldDefinitionId = field.FieldDefinitionId,
                        Value = field.Value
                    };
                    await _uow.DocumentMetadatas.AddAsync(meta);
                }
            }
            await _uow.SaveChangesAsync();
            _logger.LogInformation("Metadata saved for {Entity} ({DocId}/{FileId}/{FolderId})",
                model.EntityName, model.DocumentId, model.UserFileId, model.UserFolderId);
        }

        public async Task ApplyTemplateAsync(int templateId, int? documentId, int? userFileId, int? userFolderId)
        {
            var template = await _uow.MetadataTemplates.Query()
                .Include(t => t.Fields.Where(f => f.IsActive))
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) throw new KeyNotFoundException("Template not found.");

            foreach (var field in template.Fields)
            {
                var exists = await _uow.DocumentMetadatas.ExistsAsync(m =>
                    m.FieldDefinitionId == field.Id &&
                    ((documentId.HasValue && m.DocumentId == documentId) ||
                     (userFileId.HasValue && m.UserFileId == userFileId) ||
                     (userFolderId.HasValue && m.UserFolderId == userFolderId)));

                if (!exists)
                {
                    await _uow.DocumentMetadatas.AddAsync(new DocumentMetadata
                    {
                        DocumentId = documentId,
                        UserFileId = userFileId,
                        UserFolderId = userFolderId,
                        FieldDefinitionId = field.Id,
                        Value = field.DefaultValue
                    });
                }
            }
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  TAGS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<DocumentTagVm>> GetTagsAsync(int? documentId, int? userFileId, int? userFolderId)
        {
            return await _uow.DocumentTags.Query()
                .Where(t =>
                    (documentId.HasValue && t.DocumentId == documentId) ||
                    (userFileId.HasValue && t.UserFileId == userFileId) ||
                    (userFolderId.HasValue && t.UserFolderId == userFolderId))
                .Select(t => new DocumentTagVm { Id = t.Id, Tag = t.Tag, Color = t.Color })
                .ToListAsync();
        }

        public async Task AddTagAsync(TagAddVm model)
        {
            var exists = await _uow.DocumentTags.ExistsAsync(t =>
                t.Tag == model.Tag &&
                ((model.DocumentId.HasValue && t.DocumentId == model.DocumentId) ||
                 (model.UserFileId.HasValue && t.UserFileId == model.UserFileId) ||
                 (model.UserFolderId.HasValue && t.UserFolderId == model.UserFolderId)));

            if (exists) throw new InvalidOperationException($"Tag '{model.Tag}' already exists.");

            await _uow.DocumentTags.AddAsync(new DocumentTag
            {
                DocumentId = model.DocumentId,
                UserFileId = model.UserFileId,
                UserFolderId = model.UserFolderId,
                Tag = model.Tag,
                Color = model.Color
            });
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveTagAsync(int tagId)
        {
            var tag = await _uow.DocumentTags.GetByIdAsync(tagId);
            if (tag == null) throw new KeyNotFoundException("Tag not found.");
            _uow.DocumentTags.SoftDelete(tag);
            await _uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<string>> GetAllTagNamesAsync()
        {
            return await _uow.DocumentTags.Query()
                .Select(t => t.Tag).Distinct().OrderBy(t => t).ToListAsync();
        }

        // ═══════════════════════════════════════
        //  LINKS
        // ═══════════════════════════════════════

        public async Task<IReadOnlyList<DocumentLinkVm>> GetLinksAsync(int documentId)
        {
            var links = await _uow.DocumentLinks.Query()
                .Include(l => l.TargetDocument)
                .Where(l => l.SourceDocumentId == documentId)
                .OrderBy(l => l.LinkType).ToListAsync();

            var reverseLinks = await _uow.DocumentLinks.Query()
                .Include(l => l.SourceDocument)
                .Where(l => l.TargetDocumentId == documentId)
                .OrderBy(l => l.LinkType).ToListAsync();

            var result = links.Select(l => new DocumentLinkVm
            {
                Id = l.Id,
                TargetDocumentId = l.TargetDocumentId,
                TargetDocNumber = l.TargetDocument.DocNumber,
                TargetTitle = l.TargetDocument.Title,
                LinkType = l.LinkType,
                Description = l.Description
            }).ToList();

            result.AddRange(reverseLinks.Select(l => new DocumentLinkVm
            {
                Id = l.Id,
                TargetDocumentId = l.SourceDocumentId,
                TargetDocNumber = l.SourceDocument.DocNumber,
                TargetTitle = l.SourceDocument.Title,
                LinkType = ReverseLinkType(l.LinkType),
                Description = l.Description
            }));

            return result;
        }

        public async Task AddLinkAsync(DocumentLinkCreateVm model)
        {
            if (model.SourceDocumentId == model.TargetDocumentId)
                throw new InvalidOperationException("Cannot link a document to itself.");

            var exists = await _uow.DocumentLinks.ExistsAsync(l =>
                l.SourceDocumentId == model.SourceDocumentId
                && l.TargetDocumentId == model.TargetDocumentId
                && l.LinkType == model.LinkType);

            if (exists) throw new InvalidOperationException("This link already exists.");

            await _uow.DocumentLinks.AddAsync(new DocumentLink
            {
                SourceDocumentId = model.SourceDocumentId,
                TargetDocumentId = model.TargetDocumentId,
                LinkType = model.LinkType,
                Description = model.Description
            });
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveLinkAsync(int linkId)
        {
            var link = await _uow.DocumentLinks.GetByIdAsync(linkId);
            if (link == null) throw new KeyNotFoundException("Link not found.");
            _uow.DocumentLinks.SoftDelete(link);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  SEED SYSTEM FIELDS
        // ═══════════════════════════════════════

        public async Task SeedSystemFieldsAsync()
        {
            var systemFields = new[]
            {
                new { Name = "Author", Label = "Author", Type = MetadataFieldType.Text, Order = 1 },
                new { Name = "Owner", Label = "Document Owner", Type = MetadataFieldType.User, Order = 2 },
                new { Name = "Classification", Label = "Security Classification", Type = MetadataFieldType.Dropdown, Order = 3 },
                new { Name = "Language", Label = "Language", Type = MetadataFieldType.Dropdown, Order = 4 },
                new { Name = "RetentionPeriod", Label = "Retention Period", Type = MetadataFieldType.Dropdown, Order = 5 },
                new { Name = "RegulatoryRef", Label = "Regulatory Reference", Type = MetadataFieldType.Text, Order = 6 },
                new { Name = "ExternalRef", Label = "External Reference", Type = MetadataFieldType.Text, Order = 7 },
                new { Name = "Keywords", Label = "Keywords", Type = MetadataFieldType.TextArea, Order = 8 },
            };

            foreach (var sf in systemFields)
            {
                if (!await _uow.MetadataFieldDefinitions.ExistsAsync(f => f.FieldName == sf.Name && f.IsSystem))
                {
                    var field = new MetadataFieldDefinition
                    {
                        FieldName = sf.Name,
                        DisplayLabel = sf.Label,
                        FieldType = sf.Type,
                        SortOrder = sf.Order,
                        IsSystem = true,
                        TemplateId = null,
                        Options = sf.Name switch
                        {
                            "Classification" => "Public|Internal|Confidential|Restricted",
                            "Language" => "English|Arabic|French|Spanish|German",
                            "RetentionPeriod" => "1 Year|3 Years|5 Years|7 Years|10 Years|Permanent",
                            _ => null
                        }
                    };
                    await _uow.MetadataFieldDefinitions.AddAsync(field);
                }
            }
            await _uow.SaveChangesAsync();
            _logger.LogInformation("System metadata fields seeded");
        }

        // ── Helpers ──

        private static FieldDefinitionVm MapField(MetadataFieldDefinition f) => new()
        {
            Id = f.Id,
            FieldName = f.FieldName,
            DisplayLabel = f.DisplayLabel,
            FieldType = f.FieldType,
            Options = f.Options,
            IsRequired = f.IsRequired,
            DefaultValue = f.DefaultValue,
            Placeholder = f.Placeholder,
            SortOrder = f.SortOrder,
            IsSystem = f.IsSystem,
            HelpText = f.HelpText
        };

        private static DocumentLinkType ReverseLinkType(DocumentLinkType type) => type switch
        {
            DocumentLinkType.Supersedes => DocumentLinkType.SupersededBy,
            DocumentLinkType.SupersededBy => DocumentLinkType.Supersedes,
            DocumentLinkType.References => DocumentLinkType.ReferencedBy,
            DocumentLinkType.ReferencedBy => DocumentLinkType.References,
            DocumentLinkType.ParentOf => DocumentLinkType.ChildOf,
            DocumentLinkType.ChildOf => DocumentLinkType.ParentOf,
            _ => type
        };
    }
}
