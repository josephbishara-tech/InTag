using InTagLogicLayer.Manufacturing;

namespace InTagWeb.Configuration
{
    public static class ManufacturingServiceRegistration
    {
        public static IServiceCollection AddInTagManufacturingServices(
            this IServiceCollection services)
        {
            services.AddScoped<IManufacturingService, ManufacturingService>();
           
            return services;
        }
    }
}
