using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InTagViewModelLayer.Auth
{
    public class LoginRequestVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
