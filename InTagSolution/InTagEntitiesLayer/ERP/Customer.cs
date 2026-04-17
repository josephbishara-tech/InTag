using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.ERP
{
    public class Customer : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? TaxId { get; set; }

        public decimal CreditLimit { get; set; }

        public int PaymentTermDays { get; set; } = 30;

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        public int? PricelistId { get; set; }

        public int? SalesTeamId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public Pricelist? Pricelist { get; set; }
        public SalesTeam? SalesTeam { get; set; }
        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
