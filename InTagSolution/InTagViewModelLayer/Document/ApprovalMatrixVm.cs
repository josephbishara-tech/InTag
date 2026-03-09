using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Document
{
    public class ApprovalMatrixVm
    {
        public int Id { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentTypeDisplay => DocumentType.ToString();
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int ApproverLevel { get; set; }
        public string ApproverRole { get; set; } = null!;
        public Guid? ApproverUserId { get; set; }
        public int? EscalationHours { get; set; }
        public string? Description { get; set; }
    }

    public class ApprovalMatrixCreateVm
    {
        [Required]
        [Display(Name = "Document Type")]
        public DocumentType DocumentType { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Approval Level")]
        public int ApproverLevel { get; set; } = 1;

        [Required, MaxLength(100)]
        [Display(Name = "Approver Role")]
        public string ApproverRole { get; set; } = null!;

        public Guid? ApproverUserId { get; set; }

        [Display(Name = "Escalation (Hours)")]
        public int? EscalationHours { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
