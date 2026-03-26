using InTagViewModelLayer.Document;
using Microsoft.AspNetCore.Http;

namespace InTagLogicLayer.Document
{
    public interface IUserRepositoryService
    {
        // Folders
        Task EnsureUserRootFolderAsync(Guid userId, string userName);
        Task<RepositoryBrowseVm> BrowseMyDocumentsAsync(Guid userId, int? folderId);
        Task<RepositoryBrowseVm> BrowseSharedWithMeAsync(Guid userId);
        Task<RepositoryBrowseVm> BrowseDepartmentAsync(int departmentId, int? folderId);
        Task<FolderListVm> CreateFolderAsync(Guid userId, FolderCreateVm model);
        Task RenameFolderAsync(int folderId, string newName);
        Task DeleteFolderAsync(int folderId);

        // Files
        Task<FileListVm> UploadFileAsync(Guid userId, int folderId, IFormFile file, string? description);
        Task<(Stream Content, string FileName, string ContentType)> DownloadFileAsync(int fileId, Guid userId);
        Task DeleteFileAsync(int fileId, Guid userId);

        // Sharing
        Task ShareFileAsync(Guid userId, FileShareCreateVm model);
        Task RemoveShareAsync(int shareId, Guid userId);
        Task<IReadOnlyList<FileShareListVm>> GetFileSharesAsync(int fileId);
    }
}
