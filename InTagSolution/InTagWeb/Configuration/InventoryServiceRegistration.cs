using InTagLogicLayer.Inventory;

namespace InTagWeb.Configuration
{
    public static class InventoryServiceRegistration
    {
        public static IServiceCollection AddInTagInventoryServices(
            this IServiceCollection services)
        {
            services.AddScoped<IInventoryService, InventoryService>();
            return services;
        }
    }
}
