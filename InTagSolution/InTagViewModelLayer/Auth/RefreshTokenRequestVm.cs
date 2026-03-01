using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InTagViewModelLayer.Auth
{
    public class RefreshTokenRequestVm
    {
        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
