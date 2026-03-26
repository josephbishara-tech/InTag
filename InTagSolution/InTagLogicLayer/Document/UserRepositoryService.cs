using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public class UserRepositoryService : IUserRepositoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileStorageService _storage;
        private readonly ILogger<UserRepositoryService> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".md", ".png", ".jpg", ".jpeg", ".gif",
            ".dwg", ".dxf", ".xml", ".json", ".html", ".zip", ".rar"
        };
        private const long MaxFileSize = 50 * 1024 * 1024;

        public UserRepositoryService(IUnitOfWork uow, IFileStorageService storage,
            ILogger<UserRepositoryService> logger)
        {
            _uow = uow;
            _storage = storage;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        //  ROOT FOLDER SETUP
        // ═══════════════════════════════════════

        public async Task EnsureUserRootFolderAsync(Guid userId, string userName)
        {
            var exists = await _uow.UserFolders.ExistsAsync(
                f => f.OwnerUserId == userId && f.ParentFolderId == null && f.OwnerType == FolderOwnerType.User);

            if (!exists)
            {
                var safeName = SanitizeFolderName(userName);
                var storagePath = $"users/{userId:N}";

                var root = new UserFolder
                {
                    Name = "My Documents",
                    OwnerType = FolderOwnerType.User,
                    OwnerUserId = userId,
                    StoragePath = storagePath
                };

                await _uow.UserFolders.AddAsync(root);
                await _uow.SaveChangesAsync();
                _logger.LogInformation("Root folder created for user {UserId}", userId);
            }
        }

        // ═══════════════════════════════════════
        //  BROWSE
        // ═══════════════════════════════════════

        public async Task<RepositoryBrowseVm> BrowseMyDocumentsAsync(Guid userId, int? folderId)
        {
            UserFolder? currentFolder;

            if (folderId.HasValue)
            {
                currentFolder = await _uow.UserFolders.Query()
                    .Include(f => f.SubFolders.Where(s => s.IsActive))
                    .Include(f => f.Files.Where(fi => fi.IsActive))
                        .ThenInclude(fi => fi.Shares)
                    .FirstOrDefaultAsync(f => f.Id == folderId.Value && f.OwnerUserId == userId);
            }
            else
            {
                currentFolder = await _uow.UserFolders.Query()
                    .Include(f => f.SubFolders.Where(s => s.IsActive))
                    .Include(f => f.Files.Where(fi => fi.IsActive))
                        .ThenInclude(fi => fi.Shares)
                    .FirstOrDefaultAsync(f => f.OwnerUserId == userId
                        && f.ParentFolderId == null && f.OwnerType == FolderOwnerType.User);
            }

            if (currentFolder == null)
                throw new KeyNotFoundException("Folder not found.");

            return new RepositoryBrowseVm
            {
                CurrentFolderId = currentFolder.Id,
                CurrentFolderName = currentFolder.Name,
                CurrentPath = currentFolder.StoragePath,
                ViewMode = "my",
                Breadcrumbs = await BuildBreadcrumbsAsync(currentFolder),
                Folders = currentFolder.SubFolders
                    .OrderBy(f => f.Name)
                    .Select(f => new FolderListVm
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Description = f.Description,
                        OwnerType = f.OwnerType,
                        FileCount = f.Files?.Count(fi => fi.IsActive) ?? 0,
                        SubFolderCount = f.SubFolders?.Count(s => s.IsActive) ?? 0,
                        CreatedDate = f.CreatedDate
                    }).ToList(),
                Files = currentFolder.Files
                    .OrderByDescending(f => f.UploadedDate)
                    .Select(f => new FileListVm
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        FileType = f.FileType,
                        FileSize = f.FileSize,
                        Description = f.Description,
                        UploadedDate = f.UploadedDate,
                        ShareCount = f.Shares?.Count(s => s.IsActive) ?? 0,
                        IsSharedByMe = f.Shares?.Any(s => s.IsActive) ?? false
                    }).ToList()
            };
        }

        public async Task<RepositoryBrowseVm> BrowseSharedWithMeAsync(Guid userId)
        {
            // Get user's department for department-level shares
            var userDeptId = await GetUserDepartmentIdAsync(userId);

            var shares = await _uow.FileShares.Query()
                .Include(s => s.UserFile).ThenInclude(f => f.Folder)
                .Where(s => s.IsActive
                    && (s.TargetUserId == userId
                        || s.TargetType == ShareTargetType.Everyone
                        || (s.TargetType == ShareTargetType.Department
                            && s.TargetDepartmentId == userDeptId && userDeptId != null)))
                .OrderByDescending(s => s.SharedDate)
                .ToListAsync();

            return new RepositoryBrowseVm
            {
                ViewMode = "shared",
                CurrentFolderName = "Shared With Me",
                Files = shares.Select(s => new FileListVm
                {
                    Id = s.UserFile.Id,
                    FileName = s.UserFile.FileName,
                    FileType = s.UserFile.FileType,
                    FileSize = s.UserFile.FileSize,
                    Description = s.UserFile.Description,
                    UploadedDate = s.SharedDate,
                    UploadedByName = s.SharedByUserId.ToString()[..8] + "...",
                }).ToList()
            };
        }

        public async Task<RepositoryBrowseVm> BrowseDepartmentAsync(int departmentId, int? folderId)
        {
            UserFolder? currentFolder;

            if (folderId.HasValue)
            {
                currentFolder = await _uow.UserFolders.Query()
                    .Include(f => f.SubFolders.Where(s => s.IsActive))
                    .Include(f => f.Files.Where(fi => fi.IsActive))
                    .FirstOrDefaultAsync(f => f.Id == folderId.Value);
            }
            else
            {
                currentFolder = await _uow.UserFolders.Query()
                    .Include(f => f.SubFolders.Where(s => s.IsActive))
                    .Include(f => f.Files.Where(fi => fi.IsActive))
                    .FirstOrDefaultAsync(f => f.DepartmentId == departmentId
                        && f.ParentFolderId == null && f.OwnerType == FolderOwnerType.Department);

                // Auto-create department root folder if not exists
                if (currentFolder == null)
                {
                    var dept = await _uow.Departments.GetByIdAsync(departmentId);
                    if (dept == null) throw new KeyNotFoundException("Department not found.");

                    currentFolder = new UserFolder
                    {
                        Name = dept.Name,
                        OwnerType = FolderOwnerType.Department,
                        DepartmentId = departmentId,
                        StoragePath = $"departments/{departmentId}"
                    };
                    await _uow.UserFolders.AddAsync(currentFolder);
                    await _uow.SaveChangesAsync();
                    currentFolder.SubFolders = new List<UserFolder>();
                    currentFolder.Files = new List<UserFile>();
                }
            }

            return new RepositoryBrowseVm
            {
                CurrentFolderId = currentFolder.Id,
                CurrentFolderName = currentFolder.Name,
                ViewMode = "department",
                Breadcrumbs = await BuildBreadcrumbsAsync(currentFolder),
                Folders = currentFolder.SubFolders.OrderBy(f => f.Name)
                    .Select(MapFolder).ToList(),
                Files = currentFolder.Files.OrderByDescending(f => f.UploadedDate)
                    .Select(f => MapFile(f, false)).ToList()
            };
        }

        // ═══════════════════════════════════════
        //  FOLDERS
        // ═══════════════════════════════════════

        public async Task<FolderListVm> CreateFolderAsync(Guid userId, FolderCreateVm model)
        {
            int parentId;
            string parentPath;

            if (model.ParentFolderId.HasValue)
            {
                var parent = await _uow.UserFolders.GetByIdAsync(model.ParentFolderId.Value);
                if (parent == null) throw new KeyNotFoundException("Parent folder not found.");
                parentId = parent.Id;
                parentPath = parent.StoragePath;
            }
            else
            {
                var root = await _uow.UserFolders.Query()
                    .FirstOrDefaultAsync(f => f.OwnerUserId == userId
                        && f.ParentFolderId == null && f.OwnerType == FolderOwnerType.User);
                if (root == null) throw new InvalidOperationException("Root folder not found.");
                parentId = root.Id;
                parentPath = root.StoragePath;
            }

            var safeName = SanitizeFolderName(model.Name);

            if (await _uow.UserFolders.ExistsAsync(f => f.ParentFolderId == parentId && f.Name == model.Name))
                throw new InvalidOperationException($"Folder '{model.Name}' already exists.");

            var folder = new UserFolder
            {
                Name = model.Name,
                Description = model.Description,
                OwnerType = FolderOwnerType.User,
                OwnerUserId = userId,
                ParentFolderId = parentId,
                StoragePath = $"{parentPath}/{safeName}"
            };

            await _uow.UserFolders.AddAsync(folder);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Folder created: {Name} by user {UserId}", folder.Name, userId);
            return MapFolder(folder);
        }

        public async Task RenameFolderAsync(int folderId, string newName)
        {
            var folder = await _uow.UserFolders.GetByIdAsync(folderId);
            if (folder == null) throw new KeyNotFoundException("Folder not found.");
            if (folder.ParentFolderId == null) throw new InvalidOperationException("Cannot rename root folder.");

            folder.Name = newName;
            _uow.UserFolders.Update(folder);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(int folderId)
        {
            var folder = await _uow.UserFolders.Query()
                .Include(f => f.Files).Include(f => f.SubFolders)
                .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null) throw new KeyNotFoundException("Folder not found.");
            if (folder.ParentFolderId == null) throw new InvalidOperationException("Cannot delete root folder.");
            if (folder.SubFolders.Any(s => s.IsActive))
                throw new InvalidOperationException("Folder contains sub-folders. Delete them first.");
            if (folder.Files.Any(f => f.IsActive))
                throw new InvalidOperationException("Folder contains files. Delete them first.");

            _uow.UserFolders.SoftDelete(folder);
            await _uow.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        //  FILES
        // ═══════════════════════════════════════

        public async Task<FileListVm> UploadFileAsync(Guid userId, int folderId,
            IFormFile file, string? description)
        {
            var folder = await _uow.UserFolders.GetByIdAsync(folderId);
            if (folder == null) throw new KeyNotFoundException("Folder not found.");

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided.");
            if (file.Length > MaxFileSize)
                throw new InvalidOperationException($"File exceeds maximum size of {MaxFileSize / 1024 / 1024} MB.");

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' is not allowed.");

            using var stream = file.OpenReadStream();
            var result = await _storage.UploadAsync(stream, file.FileName, folder.StoragePath);

            var userFile = new UserFile
            {
                FolderId = folderId,
                FileName = file.FileName,
                FileType = ext.TrimStart('.').ToUpperInvariant(),
                FileSize = result.FileSize,
                StoragePath = result.StoragePath,
                Hash = result.Hash,
                Description = description,
                UploadedByUserId = userId,
                UploadedDate = DateTimeOffset.UtcNow
            };

            await _uow.UserFiles.AddAsync(userFile);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("File uploaded: {FileName} to folder {FolderId} by user {UserId}",
                file.FileName, folderId, userId);

            return MapFile(userFile, false);
        }

        public async Task<(Stream Content, string FileName, string ContentType)> DownloadFileAsync(
            int fileId, Guid userId)
        {
            var file = await _uow.UserFiles.Query()
                .Include(f => f.Folder)
                .Include(f => f.Shares)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null) throw new KeyNotFoundException("File not found.");

            // Check access: owner, or shared with user
            var isOwner = file.UploadedByUserId == userId;
            var isShared = file.Shares.Any(s => s.IsActive
                && (s.TargetUserId == userId || s.TargetType == ShareTargetType.Everyone)
                && s.Permission >= SharePermission.Download);

            // Allow folder owner access
            var folderOwnedByUser = file.Folder?.OwnerUserId == userId;

            if (!isOwner && !isShared && !folderOwnedByUser)
                throw new UnauthorizedAccessException("You don't have permission to download this file.");

            var stream = await _storage.DownloadAsync(file.StoragePath);
            var contentType = GetContentType(file.FileType);

            return (stream, file.FileName, contentType);
        }

        public async Task DeleteFileAsync(int fileId, Guid userId)
        {
            var file = await _uow.UserFiles.Query()
                .Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null) throw new KeyNotFoundException("File not found.");
            if (file.UploadedByUserId != userId && file.Folder?.OwnerUserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own files.");

            await _storage.DeleteAsync(file.StoragePath);
            _uow.UserFiles.SoftDelete(file);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("File deleted: {FileName} (ID: {Id})", file.FileName, fileId);
        }

        // ═══════════════════════════════════════
        //  SHARING
        // ═══════════════════════════════════════

        public async Task ShareFileAsync(Guid userId, FileShareCreateVm model)
        {
            var file = await _uow.UserFiles.GetByIdAsync(model.UserFileId);
            if (file == null) throw new KeyNotFoundException("File not found.");
            if (file.UploadedByUserId != userId)
                throw new UnauthorizedAccessException("You can only share your own files.");

            if (model.TargetType == ShareTargetType.User && !model.TargetUserId.HasValue)
                throw new InvalidOperationException("Target user is required.");
            if (model.TargetType == ShareTargetType.Department && !model.TargetDepartmentId.HasValue)
                throw new InvalidOperationException("Target department is required.");

            // Prevent duplicate shares
            if (model.TargetType == ShareTargetType.User)
            {
                var exists = await _uow.FileShares.ExistsAsync(s =>
                    s.UserFileId == model.UserFileId && s.TargetUserId == model.TargetUserId);
                if (exists) throw new InvalidOperationException("File is already shared with this user.");
            }

            var share = new InTagEntitiesLayer.Document.FileShare
            {
                UserFileId = model.UserFileId,
                SharedByUserId = userId,
                TargetType = model.TargetType,
                TargetUserId = model.TargetUserId,
                TargetDepartmentId = model.TargetDepartmentId,
                Permission = model.Permission,
                Message = model.Message,
                SharedDate = DateTimeOffset.UtcNow
            };

            await _uow.FileShares.AddAsync(share);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("File {FileName} shared by {UserId} to {TargetType}:{TargetId}",
                file.FileName, userId, model.TargetType, model.TargetUserId ?? (object?)model.TargetDepartmentId ?? "everyone");
        }

        public async Task RemoveShareAsync(int shareId, Guid userId)
        {
            var share = await _uow.FileShares.GetByIdAsync(shareId);
            if (share == null) throw new KeyNotFoundException("Share not found.");
            if (share.SharedByUserId != userId)
                throw new UnauthorizedAccessException("You can only remove your own shares.");

            _uow.FileShares.SoftDelete(share);
            await _uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<FileShareListVm>> GetFileSharesAsync(int fileId)
        {
            var shares = await _uow.FileShares.Query()
                .Include(s => s.TargetDepartment)
                .Where(s => s.UserFileId == fileId)
                .OrderByDescending(s => s.SharedDate)
                .ToListAsync();

            return shares.Select(s => new FileShareListVm
            {
                ShareId = s.Id,
                TargetName = s.TargetType switch
                {
                    ShareTargetType.User => s.TargetUserId?.ToString()?[..8] + "..." ?? "Unknown",
                    ShareTargetType.Department => s.TargetDepartment?.Name ?? "Unknown Dept",
                    ShareTargetType.Everyone => "Everyone",
                    _ => "Unknown"
                },
                TargetType = s.TargetType,
                Permission = s.Permission,
                SharedDate = s.SharedDate
            }).ToList();
        }

        // ═══════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════

        private async Task<IReadOnlyList<BreadcrumbVm>> BuildBreadcrumbsAsync(UserFolder folder)
        {
            var crumbs = new List<BreadcrumbVm>();
            var current = folder;
            while (current != null)
            {
                crumbs.Insert(0, new BreadcrumbVm { FolderId = current.ParentFolderId.HasValue ? current.Id : null, Name = current.Name });
                if (current.ParentFolderId.HasValue)
                    current = await _uow.UserFolders.GetByIdAsync(current.ParentFolderId.Value);
                else
                    current = null;
            }
            return crumbs;
        }

        private async Task<int?> GetUserDepartmentIdAsync(Guid userId)
        {
            // This is a simplified lookup — adjust based on your ApplicationUser entity
            return null; // TODO: Query ApplicationUser.DepartmentId if available
        }

        private static FolderListVm MapFolder(UserFolder f) => new()
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            OwnerType = f.OwnerType,
            FileCount = f.Files?.Count(fi => fi.IsActive) ?? 0,
            SubFolderCount = f.SubFolders?.Count(s => s.IsActive) ?? 0,
            CreatedDate = f.CreatedDate
        };

        private static FileListVm MapFile(UserFile f, bool isShared) => new()
        {
            Id = f.Id,
            FileName = f.FileName,
            FileType = f.FileType,
            FileSize = f.FileSize,
            Description = f.Description,
            UploadedDate = f.UploadedDate,
            ShareCount = f.Shares?.Count(s => s.IsActive) ?? 0,
            IsSharedByMe = isShared
        };

        private static string SanitizeFolderName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        private static string GetContentType(string fileType) => fileType.ToUpper() switch
        {
            "PDF" => "application/pdf",
            "DOCX" or "DOC" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "XLSX" or "XLS" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PPTX" or "PPT" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "PNG" => "image/png",
            "JPG" or "JPEG" => "image/jpeg",
            "GIF" => "image/gif",
            "ZIP" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
