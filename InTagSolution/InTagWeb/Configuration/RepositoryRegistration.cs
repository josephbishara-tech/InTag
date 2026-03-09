using InTagRepositoryLayer.Common;

namespace InTagWeb.Configuration
{
    public static class RepositoryRegistration
    {
        public static IServiceCollection AddInTagRepositories(
            this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}