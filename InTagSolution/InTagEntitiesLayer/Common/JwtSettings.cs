using System;
using System.Collections.Generic;
using System.Text;

namespace InTagEntitiesLayer.Common
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
