using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.Document
{
    public class UserFolder : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Owner type: User, Product, Department
        /// </summary>
        [Required]
        public FolderOwnerType OwnerType { get; set; }

        /// <summary>
        /// UserId for personal folders
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// ProductId for product folders
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// DepartmentId for department folders
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Parent folder for hierarchy
        /// </summary>
        public int? ParentFolderId { get; set; }

        /// <summary>
        /// Physical path relative to InTagFiles root
        /// </summary>
        [Required, MaxLength(1000)]
        public string StoragePath { get; set; } = null!;

        // Navigation
        public UserFolder? ParentFolder { get; set; }
        public ICollection<UserFolder> SubFolders { get; set; } = new List<UserFolder>();
        public ICollection<UserFile> Files { get; set; } = new List<UserFile>();
        public Asset.Department? Department { get; set; }
    }
}
