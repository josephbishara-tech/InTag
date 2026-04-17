using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.ERP
{
    public class CustomerListVm
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal CreditLimit { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CustomerCreateVm
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [MaxLength(200), EmailAddress]
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
        [Display(Name = "Tax ID")]
        public string? TaxId { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Credit Limit")]
        public decimal CreditLimit { get; set; }

        [Range(0, 365)]
        [Display(Name = "Payment Terms (days)")]
        public int PaymentTermDays { get; set; } = 30;

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Pricelist")]
        public int? PricelistId { get; set; }

        [Display(Name = "Sales Team")]
        public int? SalesTeamId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CustomerUpdateVm : CustomerCreateVm
    {
        public int Id { get; set; }
    }

    public class CustomerDetailVm : CustomerListVm
    {
        public string? Address { get; set; }
        public string? TaxId { get; set; }
        public int PaymentTermDays { get; set; }
        public string? PricelistName { get; set; }
        public string? SalesTeamName { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
