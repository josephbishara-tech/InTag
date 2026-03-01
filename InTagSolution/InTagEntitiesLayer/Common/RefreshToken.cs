using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InTagEntitiesLayer.Common
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ExpiresDate { get; set; }

        public DateTimeOffset? RevokedDate { get; set; }

        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresDate;

        public bool IsRevoked => RevokedDate != null;

        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }

}
