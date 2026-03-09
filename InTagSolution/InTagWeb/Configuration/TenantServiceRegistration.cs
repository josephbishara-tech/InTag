using InTagDataLayer.Context;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Services;
using Microsoft.EntityFrameworkCore;

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

        public static async Task SeedDefaultTenantAsync(IServiceProvider serviceProvider)
        {
            var catalogDb = serviceProvider.GetRequiredService<CatalogDbContext>();
            await catalogDb.Database.EnsureCreatedAsync();

            if (!await catalogDb.Tenants.AnyAsync())
            {
                catalogDb.Tenants.Add(new Tenant
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "InTag Demo",
                    Subdomain = "localhost",
                    CustomDomain = "localhost",
                    Tier = SubscriptionTier.Enterprise,
                    Status = TenantStatus.Active,
                    IsolationStrategy = TenantIsolationStrategy.SharedSchema,
                    FeatureFlags = "{\"Asset\":true,\"Document\":true,\"Manufacturing\":true,\"Maintenance\":true,\"Inventory\":true,\"Workflow\":true}",
                    CreatedDate = DateTimeOffset.UtcNow
                });

                await catalogDb.SaveChangesAsync();
            }
        }
    }
}