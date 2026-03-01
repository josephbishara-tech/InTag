using Microsoft.EntityFrameworkCore;
using InTagDataLayer.Context;
using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Services;

namespace InTagWeb.Configuration
{
    public static class TenantServiceRegistration
    {
        public static IServiceCollection AddInTagMultiTenancy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Catalog database (shared across all tenants)
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CatalogConnection")));

            // Tenant services — scoped (one per request)
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ITenantResolutionService, TenantResolutionService>();

            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            return services;
        }
    }
}