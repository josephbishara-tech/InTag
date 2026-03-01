using System;
using System.Collections.Generic;
using System.Text;

namespace InTagViewModelLayer.Auth
{
    public class AuthResponseVm
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTimeOffset AccessTokenExpiration { get; set; }
        public UserInfoVm User { get; set; } = null!;
    }

    public class UserInfoVm
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid TenantId { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
