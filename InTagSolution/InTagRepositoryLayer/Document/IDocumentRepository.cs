using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Document
{
    public interface IDocumentRepository : IGenericRepository<InTagEntitiesLayer.Document.Document>
    {
        Task<InTagEntitiesLayer.Document.Document?> GetWithRevisionsAsync(int id);
        Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> GetByStatusAsync(DocumentStatus status);
        Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> GetDueForReviewAsync();
        Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> GetCheckedOutAsync();
        Task<bool> DocNumberExistsAsync(string docNumber);
        Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> SearchAsync(
            string searchTerm, int page, int pageSize);
        Task<int> SearchCountAsync(string searchTerm);
    }
}
