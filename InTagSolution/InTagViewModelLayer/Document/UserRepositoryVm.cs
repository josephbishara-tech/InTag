using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    // ── Browse View ──
    public class RepositoryBrowseVm
    {
        public string CurrentPath { get; set; } = "/";
        public int? CurrentFolderId { get; set; }
        public string? CurrentFolderName { get; set; }
        public IReadOnlyList<BreadcrumbVm> Breadcrumbs { get; set; } = new List<BreadcrumbVm>();
        public IReadOnlyList<FolderListVm> Folders { get; set; } = new List<FolderListVm>();
        public IReadOnlyList<FileListVm> Files { get; set; } = new List<FileListVm>();
        public string ViewMode { get; set; } = "my"; // my, shared, department, product
    }

    public class BreadcrumbVm
    {
        public int? FolderId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class FolderListVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public FolderOwnerType OwnerType { get; set; }
        public int FileCount { get; set; }
        public int SubFolderCount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class FileListVm
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileSizeDisplay => FileSize switch
        {
            < 1024 => $"{FileSize} B",
            < 1024 * 1024 => $"{FileSize / 1024} KB",
            _ => $"{FileSize / 1024.0 / 1024.0:N1} MB"
        };
        public string FileIcon => FileType?.ToUpper() switch
        {
            "PDF" => "bi-file-earmark-pdf",
            "DOC" or "DOCX" => "bi-file-earmark-word",
            "XLS" or "XLSX" => "bi-file-earmark-excel",
            "PPT" or "PPTX" => "bi-file-earmark-ppt",
            "PNG" or "JPG" or "JPEG" or "GIF" => "bi-file-earmark-image",
            "DWG" or "DXF" => "bi-file-earmark-ruled",
            "ZIP" or "RAR" => "bi-file-earmark-zip",
            _ => "bi-file-earmark"
        };
        public string? Description { get; set; }
        public string? UploadedByName { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
        public int ShareCount { get; set; }
        public bool IsSharedByMe { get; set; }
    }

    // ── Create / Edit ──
    public class FolderCreateVm
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentFolderId { get; set; }
    }

    public class FileUploadToFolderVm
    {
        [Required]
        public int FolderId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    // ── Sharing ──
    public class FileShareCreateVm
    {
        [Required]
        public int UserFileId { get; set; }

        [Required]
        public ShareTargetType TargetType { get; set; }

        public Guid? TargetUserId { get; set; }

        public int? TargetDepartmentId { get; set; }

        public SharePermission Permission { get; set; } = SharePermission.View;

        [MaxLength(500)]
        public string? Message { get; set; }
    }

    public class SharedFileVm
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileSizeDisplay => FileSize switch
        {
            < 1024 => $"{FileSize} B",
            < 1024 * 1024 => $"{FileSize / 1024} KB",
            _ => $"{FileSize / 1024.0 / 1024.0:N1} MB"
        };
        public string FileIcon => FileType?.ToUpper() switch
        {
            "PDF" => "bi-file-earmark-pdf",
            "DOC" or "DOCX" => "bi-file-earmark-word",
            "XLS" or "XLSX" => "bi-file-earmark-excel",
            "PNG" or "JPG" or "JPEG" => "bi-file-earmark-image",
            _ => "bi-file-earmark"
        };
        public string SharedByName { get; set; } = null!;
        public SharePermission Permission { get; set; }
        public string PermissionDisplay => Permission.ToString();
        public string? Message { get; set; }
        public DateTimeOffset SharedDate { get; set; }
        public string? FolderName { get; set; }
    }

    public class FileShareListVm
    {
        public int ShareId { get; set; }
        public string TargetName { get; set; } = null!;
        public ShareTargetType TargetType { get; set; }
        public SharePermission Permission { get; set; }
        public DateTimeOffset SharedDate { get; set; }
    }
}
