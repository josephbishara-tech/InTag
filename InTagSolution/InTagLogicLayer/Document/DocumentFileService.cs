using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public class DocumentFileService : IDocumentFileService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileStorageService _storage;
        private readonly ILogger<DocumentFileService> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".md", ".png", ".jpg", ".jpeg", ".gif",
            ".dwg", ".dxf", ".xml", ".json", ".html", ".zip"
        };

        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

        public DocumentFileService(
            IUnitOfWork uow,
            IFileStorageService storage,
            ILogger<DocumentFileService> logger)
        {
            _uow = uow;
            _storage = storage;
            _logger = logger;
        }

        public async Task<FileUploadResultVm> UploadFileAsync(int revisionId, IFormFile file)
        {
            // Validate revision exists
            var revision = await _uow.DocumentRevisions.GetByIdAsync(revisionId);
            if (revision == null)
                throw new KeyNotFoundException("Revision not found.");

            // Validate file
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided.");

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException($"File exceeds maximum size of {MaxFileSize / 1024 / 1024} MB.");

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' is not allowed.");

            // Upload to storage
            using var stream = file.OpenReadStream();
            var folder = $"documents/{revision.DocumentId}/rev-{revisionId}";
            var result = await _storage.UploadAsync(stream, file.FileName, folder);

            // Extract content for search indexing
            stream.Position = 0;
            var contentIndex = await ContentExtractor.ExtractAsync(stream, file.FileName);

            // Create file record
            var docFile = new DocumentFile
            {
                RevisionId = revisionId,
                FileName = file.FileName,
                FileType = ext.TrimStart('.').ToUpperInvariant(),
                FileSize = result.FileSize,
                StoragePath = result.StoragePath,
                Hash = result.Hash,
                ContentIndex = contentIndex
            };

            await _uow.DocumentFiles.AddAsync(docFile);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("File uploaded: {FileName} ({Size} bytes) for revision {RevId}",
                file.FileName, result.FileSize, revisionId);

            return new FileUploadResultVm
            {
                FileId = docFile.Id,
                FileName = docFile.FileName,
                FileType = docFile.FileType,
                FileSize = docFile.FileSize,
                Hash = docFile.Hash,
                ContentIndexed = contentIndex != null
            };
        }

        public async Task<FileDownloadVm> DownloadFileAsync(int fileId)
        {
            var file = await _uow.DocumentFiles.GetByIdAsync(fileId);
            if (file == null)
                throw new KeyNotFoundException("File not found.");

            var stream = await _storage.DownloadAsync(file.StoragePath);

            var contentType = file.FileType.ToUpperInvariant() switch
            {
                "PDF" => "application/pdf",
                "DOCX" or "DOC" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "XLSX" or "XLS" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PNG" => "image/png",
                "JPG" or "JPEG" => "image/jpeg",
                _ => "application/octet-stream"
            };

            return new FileDownloadVm
            {
                Content = stream,
                FileName = file.FileName,
                ContentType = contentType
            };
        }

        public async Task DeleteFileAsync(int fileId)
        {
            var file = await _uow.DocumentFiles.GetByIdAsync(fileId);
            if (file == null)
                throw new KeyNotFoundException("File not found.");

            await _storage.DeleteAsync(file.StoragePath);
            _uow.DocumentFiles.SoftDelete(file);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("File deleted: {FileName} (ID: {FileId})", file.FileName, fileId);
        }

        public async Task<DocumentSearchResultVm> FullTextSearchAsync(
            string searchTerm, int page, int pageSize)
        {
            var term = searchTerm.Trim();

            // Search metadata (doc number, title, tags, description)
            var metadataHits = await _uow.Documents.Query()
                .Where(d => d.DocNumber.Contains(term)
                            || d.Title.Contains(term)
                            || (d.Tags != null && d.Tags.Contains(term))
                            || (d.Description != null && d.Description.Contains(term)))
                .Select(d => new DocumentSearchHitVm
                {
                    DocumentId = d.Id,
                    DocNumber = d.DocNumber,
                    Title = d.Title,
                    Snippet = d.Description,
                    FileName = "",
                    MatchSource = "metadata"
                })
                .ToListAsync();

            // Search file content
            var contentHits = await _uow.DocumentFiles.Query()
                .Where(f => f.ContentIndex != null && f.ContentIndex.Contains(term))
                .Include(f => f.Revision)
                    .ThenInclude(r => r.Document)
                .Select(f => new DocumentSearchHitVm
                {
                    DocumentId = f.Revision.DocumentId,
                    DocNumber = f.Revision.Document.DocNumber,
                    Title = f.Revision.Document.Title,
                    Snippet = ExtractSnippet(f.ContentIndex!, term),
                    FileName = f.FileName,
                    MatchSource = "content"
                })
                .ToListAsync();

            // Merge and deduplicate
            var allHits = metadataHits.Concat(contentHits)
                .GroupBy(h => h.DocumentId)
                .Select(g => g.First())
                .OrderBy(h => h.DocNumber)
                .ToList();

            var totalCount = allHits.Count;
            var pagedHits = allHits
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new DocumentSearchResultVm
            {
                Hits = pagedHits,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static string? ExtractSnippet(string content, string term)
        {
            var idx = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            var start = Math.Max(0, idx - 80);
            var end = Math.Min(content.Length, idx + term.Length + 80);
            var snippet = content[start..end].Trim();

            if (start > 0) snippet = "..." + snippet;
            if (end < content.Length) snippet += "...";

            return snippet;
        }
    }
}
