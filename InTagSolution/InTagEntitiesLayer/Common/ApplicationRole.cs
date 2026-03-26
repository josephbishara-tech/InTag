using Microsoft.AspNetCore.Identity;

namespace InTagEntitiesLayer.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }
}
