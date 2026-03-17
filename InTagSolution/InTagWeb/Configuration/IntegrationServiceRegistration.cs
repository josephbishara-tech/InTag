using InTagLogicLayer.Integration;

namespace InTagWeb.Configuration
{
    public static class IntegrationServiceRegistration
    {
        public static IServiceCollection AddInTagIntegrationServices(
            this IServiceCollection services)
        {
            services.AddScoped<IIntegrationService, IntegrationService>();
            return services;
        }
    }
}
