using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;

namespace InTagRepositoryLayer.Document
{
    public class DocumentRepository : GenericRepository<InTagEntitiesLayer.Document.Document>, IDocumentRepository
    {
        public DocumentRepository(InTagDbContext context) : base(context) { }

        public async Task<InTagEntitiesLayer.Document.Document?> GetWithRevisionsAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Department)
                .Include(d => d.Revisions.OrderByDescending(r => r.CreatedDate))
                    .ThenInclude(r => r.Files)
                .Include(d => d.DistributionRecords)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> GetByStatusAsync(DocumentStatus status)
            => await _dbSet.Where(d => d.Status == status)
                .Include(d => d.Department)
                .OrderBy(d => d.DocNumber)
                .ToListAsync();

        public async Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> GetDueForReviewAsync()
            => await _dbSet
                .Where(d => d.NextReviewDate.HasValue
                            && d.NextReviewDate <= DateTimeOffset.UtcNow.AddDays(30)
                            && d.Status == DocumentStatus.Published)
                .OrderBy(d => d.NextReviewDate)
                .Include(d => d.Department)
                .ToListAsync();

        public async Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> GetCheckedOutAsync()
            => await _dbSet.Where(d => d.IsCheckedOut)
                .OrderBy(d => d.CheckedOutDate)
                .ToListAsync();

        public async Task<bool> DocNumberExistsAsync(string docNumber)
            => await _dbSet.AnyAsync(d => d.DocNumber == docNumber);

        public async Task<IReadOnlyList<InTagEntitiesLayer.Document.Document>> SearchAsync(
            string searchTerm, int page, int pageSize)
        {
            return await _dbSet
                .Where(d => d.DocNumber.Contains(searchTerm)
                            || d.Title.Contains(searchTerm)
                            || (d.Tags != null && d.Tags.Contains(searchTerm))
                            || (d.Description != null && d.Description.Contains(searchTerm)))
                .Include(d => d.Department)
                .OrderBy(d => d.DocNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> SearchCountAsync(string searchTerm)
        {
            return await _dbSet.CountAsync(d =>
                d.DocNumber.Contains(searchTerm)
                || d.Title.Contains(searchTerm)
                || (d.Tags != null && d.Tags.Contains(searchTerm)));
        }
    }
}
