using Serilog.Context;
using InTagEntitiesLayer.Interfaces;

namespace InTagWeb.Middleware
{
    public class LogContextEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public LogContextEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            var tenantId = "unknown";
            var userId = "anonymous";

            try
            {
                var tenant = tenantService.GetCurrentTenant();
                if (tenant != null) tenantId = tenant.Id.ToString();
            }
            catch { /* Tenant not yet resolved */ }

            try
            {
                var uid = tenantService.GetCurrentUserId();
                if (uid != Guid.Empty) userId = uid.ToString();
            }
            catch { /* User not yet authenticated */ }

            using (LogContext.PushProperty("TenantId", tenantId))
            using (LogContext.PushProperty("UserId", userId))
            {
                await _next(context);
            }
        }
    }

    public static class LogContextEnrichmentMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogContextEnrichment(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogContextEnrichmentMiddleware>();
        }
    }
}