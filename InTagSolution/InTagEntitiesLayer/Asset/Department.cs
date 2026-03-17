using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Asset
{
    public class Department : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        public string? Code { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
       
        // Navigation
        public Department? ParentDepartment { get; set; }
        public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
        public ICollection<AssetItem> Assets { get; set; } = new List<AssetItem>();
    }
}
