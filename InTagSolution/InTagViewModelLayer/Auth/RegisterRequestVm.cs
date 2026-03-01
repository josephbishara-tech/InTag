using System.ComponentModel.DataAnnotations;
namespace InTagViewModelLayer.Auth
{
    public class RegisterRequestVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(8)]
        public string Password { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public Guid TenantId { get; set; }
    }
}
