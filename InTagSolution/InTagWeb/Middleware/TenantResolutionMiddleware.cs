using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Services;

namespace InTagWeb.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        // Paths that don't require tenant resolution
        private static readonly string[] ExcludedPaths =
        {
            "/health",
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/auth/refresh"
        };

        public TenantResolutionMiddleware(RequestDelegate next,
            ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITenantResolutionService resolutionService,
            ITenantService tenantService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Skip tenant resolution for excluded paths
            if (ExcludedPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            var host = context.Request.Host.Host;
            var tenant = await resolutionService.ResolveFromHostAsync(host);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant could not be resolved for host: {Host}", host);
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { "Tenant not found. Please check your URL." }
                });
                return;
            }

            if (!tenant.IsActive)
            {
                _logger.LogWarning("Inactive tenant attempted access: {TenantId} ({Subdomain})",
                    tenant.Id, tenant.Subdomain);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { "Your subscription is inactive. Please contact support." }
                });
                return;
            }

            // Set the resolved tenant for this request
            tenantService.SetCurrentTenant(tenant);

            // Add tenant info to HttpContext.Items for convenience
            context.Items["TenantId"] = tenant.Id;
            context.Items["TenantTier"] = tenant.Tier;

            _logger.LogDebug("Tenant resolved: {TenantName} ({TenantId}) via {Host}",
                tenant.Name, tenant.Id, host);

            await _next(context);
        }
    }
    public static class TenantResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolutionMiddleware>();
        }
    }
}
