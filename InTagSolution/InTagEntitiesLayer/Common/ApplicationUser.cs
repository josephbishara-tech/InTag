using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InTagEntitiesLayer.Common
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required]
        public Guid TenantId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? JobTitle { get; set; }


        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedDate { get; set; }

    }
}
