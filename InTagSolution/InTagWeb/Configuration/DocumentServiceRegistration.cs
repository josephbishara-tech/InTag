using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Document;
using InTagLogicLayer.Services;

namespace InTagWeb.Configuration
{
    public static class DocumentServiceRegistration
    {
        public static IServiceCollection AddInTagDocumentServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentFileService, DocumentFileService>();
            services.AddScoped<IApprovalMatrixService, ApprovalMatrixService>();

            // File storage — local disk for VPS/IIS deployment
            var basePath = configuration.GetValue<string>("FileStorage:LocalPath")
                           ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "files");
            services.AddSingleton<IFileStorageService>(new LocalFileStorageService(basePath));

            return services;
        }
    }
}
