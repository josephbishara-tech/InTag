using InTagEntitiesLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InTagEntitiesLayer.Common
{
    /// <summary>
    /// Stored in the shared catalog database.
    /// Holds tenant metadata, subscription status, and feature flags.
    /// </summary>
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Subdomain identifier: e.g. "acme" for acme.intag.io
        /// </summary>
        [Required, MaxLength(63)]
        public string Subdomain { get; set; } = null!;

        /// <summary>
        /// Optional custom domain: e.g. "assets.acmecorp.com"
        /// </summary>
        [MaxLength(253)]
        public string? CustomDomain { get; set; }

        [Required]
        public SubscriptionTier Tier { get; set; }

        [Required]
        public TenantStatus Status { get; set; } = TenantStatus.Active;

        [Required]
        public TenantIsolationStrategy IsolationStrategy { get; set; }

        /// <summary>
        /// Connection string override for Enterprise (database-per-tenant).
        /// Null for shared schema tenants.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// JSON-serialized feature flags for per-module licensing.
        /// e.g. {"Asset":true,"Document":true,"Manufacturing":false,...}
        /// </summary>
        [Required]
        public string FeatureFlags { get; set; } = "{}";

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? SubscriptionExpiryDate { get; set; }

        public bool IsActive => Status == TenantStatus.Active || Status == TenantStatus.Trial;
    }
}
