using InTagViewModelLayer.Document;
using Microsoft.AspNetCore.Http;

namespace InTagLogicLayer.Document
{
    public interface IDocumentFileService
    {
        Task<FileUploadResultVm> UploadFileAsync(int revisionId, IFormFile file);
        Task<FileDownloadVm> DownloadFileAsync(int fileId);
        Task DeleteFileAsync(int fileId);
        Task<DocumentSearchResultVm> FullTextSearchAsync(string searchTerm, int page, int pageSize);
    }
}
