using InTagLogicLayer.Asset;

namespace InTagWeb.Configuration
{
    public static class AssetServiceRegistration
    {
        public static IServiceCollection AddInTagAssetServices(
            this IServiceCollection services)
        {
            services.AddScoped<IAssetService, AssetService>();
            return services;
        }
    }
}