using InTagDataLayer.Context;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace InTagLogicLayer.Services
{
    public interface ITenantResolutionService
    {
        Task<Tenant?> ResolveBySubdomainAsync(string subdomain);
        Task<Tenant?> ResolveByCustomDomainAsync(string domain);
        Task<Tenant?> ResolveFromHostAsync(string host);
    }

    public class TenantResolutionService : ITenantResolutionService
    {
        private readonly CatalogDbContext _catalogDb;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public TenantResolutionService(CatalogDbContext catalogDb, IMemoryCache cache)
        {
            _catalogDb = catalogDb;
            _cache = cache;
        }

        public async Task<Tenant?> ResolveFromHostAsync(string host)
        {
            // Remove port (e.g. "localhost:5001" → "localhost")
            var hostOnly = host.Split(':')[0];

            // 1. Try custom domain first (handles localhost too)
            var tenant = await ResolveByCustomDomainAsync(hostOnly);
            if (tenant != null) return tenant;

            // 2. Try subdomain match (e.g. "acme.intag.io")
            tenant = await ResolveBySubdomainAsync(hostOnly);
            if (tenant != null) return tenant;

            // 3. Try subdomain extraction (e.g. extract "acme" from "acme.intag.io")
            var subdomain = ExtractSubdomain(host);
            if (!string.IsNullOrEmpty(subdomain))
            {
                tenant = await ResolveBySubdomainAsync(subdomain);
            }

            return tenant;
        }

        public async Task<Tenant?> ResolveBySubdomainAsync(string subdomain)
        {
            var cacheKey = $"tenant:subdomain:{subdomain.ToLowerInvariant()}";

            if (_cache.TryGetValue(cacheKey, out Tenant? cached))
                return cached;

            var tenant = await _catalogDb.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain
                                          && (t.Status == TenantStatus.Active
                                              || t.Status == TenantStatus.Trial));

            if (tenant != null)
                _cache.Set(cacheKey, tenant, CacheDuration);

            return tenant;
        }

        public async Task<Tenant?> ResolveByCustomDomainAsync(string domain)
        {
            var cacheKey = $"tenant:domain:{domain.ToLowerInvariant()}";

            if (_cache.TryGetValue(cacheKey, out Tenant? cached))
                return cached;

            var tenant = await _catalogDb.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.CustomDomain == domain
                                          && (t.Status == TenantStatus.Active
                                              || t.Status == TenantStatus.Trial));

            if (tenant != null)
                _cache.Set(cacheKey, tenant, CacheDuration);

            return tenant;
        }

        /// <summary>
        /// Extracts subdomain from host. 
        /// e.g. "acme.intag.io" → "acme", "intag.io" → null, "localhost:5000" → null
        /// </summary>
        private static string? ExtractSubdomain(string host)
        {
            // Remove port if present
            var hostOnly = host.Split(':')[0];
            var parts = hostOnly.Split('.');

            // Must have at least 3 parts: subdomain.domain.tld
            // Skip "www" as a subdomain
            if (parts.Length >= 3 && parts[0] != "www")
                return parts[0].ToLowerInvariant();

            return null;
        }
    }
}
