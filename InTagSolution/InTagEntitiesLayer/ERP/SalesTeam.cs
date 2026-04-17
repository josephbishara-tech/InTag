using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.ERP
{
    public class SalesTeam : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public Guid? LeaderUserId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal DefaultCommissionPercent { get; set; }

        // Navigation
        public ICollection<CommissionRule> CommissionRules { get; set; } = new List<CommissionRule>();
    }

    public class CommissionRule : BaseEntity
    {
        [Required]
        public int SalesTeamId { get; set; }

        public Guid? SalespersonUserId { get; set; }

        public decimal CommissionPercent { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public decimal? MaxOrderAmount { get; set; }

        public int? ProductCategoryId { get; set; }

        // Navigation
        public SalesTeam SalesTeam { get; set; } = null!;
    }
}
