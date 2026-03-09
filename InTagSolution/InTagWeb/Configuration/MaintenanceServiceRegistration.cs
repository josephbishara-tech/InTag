using InTagLogicLayer.Maintenance;

namespace InTagWeb.Configuration
{
    public static class MaintenanceServiceRegistration
    {
        public static IServiceCollection AddInTagMaintenanceServices(
            this IServiceCollection services)
        {
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            return services;
        }
    }
}
