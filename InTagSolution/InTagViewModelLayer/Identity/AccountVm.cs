using System.ComponentModel.DataAnnotations;

namespace InTagViewModelLayer.Identity
{
    public class LoginVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class ChangePasswordVm
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password), Compare("NewPassword")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class UserProfileVm
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class CreateUserVm
    {
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Operator";
    }

    public class EditUserVm
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = null!;

        public bool IsActive { get; set; }
    }

    public class ResetPasswordVm
    {
        public Guid UserId { get; set; }

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password), Compare("NewPassword")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
