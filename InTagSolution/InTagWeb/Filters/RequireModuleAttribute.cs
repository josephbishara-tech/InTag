using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;

namespace InTagWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireModuleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly PlatformModule _module;

        public RequireModuleAttribute(PlatformModule module)
        {
            _module = module;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var tenantService = context.HttpContext.RequestServices
                .GetRequiredService<ITenantService>();

            var tenant = tenantService.GetCurrentTenant();

            if (tenant == null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    errors = new[] { "Tenant not resolved." }
                });
                return;
            }

            // Parse feature flags JSON
            var flags = JsonSerializer.Deserialize<Dictionary<string, bool>>(
                tenant.FeatureFlags) ?? new();

            var moduleName = _module.ToString();

            if (!flags.TryGetValue(moduleName, out var enabled) || !enabled)
            {
                context.Result = new ObjectResult(new
                {
                    errors = new[] { $"Module '{moduleName}' is not included in your subscription." }
                })
                {
                    StatusCode = 403
                };
                return;
            }

            await Task.CompletedTask;
        }
    }
}