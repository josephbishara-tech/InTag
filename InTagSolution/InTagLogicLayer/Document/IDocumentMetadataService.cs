using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public interface IDocumentMetadataService
    {
        // Templates
        Task<IReadOnlyList<MetadataTemplateListVm>> GetTemplatesAsync();
        Task<MetadataTemplateDetailVm> GetTemplateByIdAsync(int id);
        Task<MetadataTemplateDetailVm> CreateTemplateAsync(MetadataTemplateCreateVm model);
        Task DeleteTemplateAsync(int id);

        // Field Definitions
        Task<FieldDefinitionVm> AddFieldToTemplateAsync(FieldDefinitionCreateVm model);
        Task RemoveFieldAsync(int fieldId);

        // Metadata Values — get/save for any entity
        Task<MetadataEditVm> GetMetadataAsync(int? documentId, int? userFileId, int? userFolderId);
        Task SaveMetadataAsync(MetadataEditVm model);
        Task ApplyTemplateAsync(int templateId, int? documentId, int? userFileId, int? userFolderId);

        // Tags
        Task<IReadOnlyList<DocumentTagVm>> GetTagsAsync(int? documentId, int? userFileId, int? userFolderId);
        Task AddTagAsync(TagAddVm model);
        Task RemoveTagAsync(int tagId);
        Task<IReadOnlyList<string>> GetAllTagNamesAsync();

        // Links
        Task<IReadOnlyList<DocumentLinkVm>> GetLinksAsync(int documentId);
        Task AddLinkAsync(DocumentLinkCreateVm model);
        Task RemoveLinkAsync(int linkId);

        // Seed default system fields
        Task SeedSystemFieldsAsync();
    }
}
