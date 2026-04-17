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
            services.AddScoped<IUserRepositoryService, UserRepositoryService>();
            services.AddScoped<IDocumentMetadataService, DocumentMetadataService>();

            // File storage — register as pre-built singleton
            var basePath = configuration.GetValue<string>("FileStorage:LocalPath") ?? "wwwroot/InTagFiles";
            var fullPath = Path.IsPathRooted(basePath)
                ? basePath
                : Path.Combine(Directory.GetCurrentDirectory(), basePath);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            services.AddSingleton<IFileStorageService>(
                _ => new LocalFileStorageService(fullPath));

            return services;
        }
    }
}
