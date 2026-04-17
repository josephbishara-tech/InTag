using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;

namespace InTagEntitiesLayer.ERP
{
    public class Account : BaseEntity
    {
        [Required, MaxLength(20)]
        public string AccountCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public AccountType AccountType { get; set; }

        public int? ParentAccountId { get; set; }

        public bool IsReconcilable { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public Account? ParentAccount { get; set; }
        public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
    }

    public class CostCenter : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        public int? ParentCostCenterId { get; set; }

        public CostCenter? ParentCostCenter { get; set; }
    }
}
