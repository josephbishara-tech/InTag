using InTagLogicLayer.ERP;

namespace InTagWeb.Configuration
{
    public static class ErpServiceRegistration
    {
        public static IServiceCollection AddInTagErpServices(this IServiceCollection services)
        {
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<IPurchaseService, PurchaseService>();
            return services;
        }
    }
}
