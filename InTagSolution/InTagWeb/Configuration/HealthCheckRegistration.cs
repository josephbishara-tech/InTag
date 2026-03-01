using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InTagWeb.Configuration
{
    public static class HealthCheckRegistration
    {
        public static IServiceCollection AddInTagHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks()
                // Main tenant database
                .AddSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection")!,
                    name: "sqlserver-main",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql", "main" })
                // Catalog database
                .AddSqlServer(
                    connectionString: configuration.GetConnectionString("CatalogConnection")!,
                    name: "sqlserver-catalog",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql", "catalog" })
                // Custom: check disk space
                .AddCheck("disk-space", new DiskSpaceHealthCheck(),
                    tags: new[] { "system" })
                // Custom: check memory
                .AddCheck("memory", new MemoryHealthCheck(),
                    tags: new[] { "system" });

            return services;
        }

        public static WebApplication MapInTagHealthChecks(this WebApplication app)
        {
            // Detailed health check endpoint (for monitoring tools)
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                AllowCachingResponses = false
            });

            // Lightweight liveness probe (for load balancers)
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false // No checks — just confirms app is running
            });

            // Database-only readiness probe
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("db")
            });

            return app;
        }
    }

    /// <summary>
    /// Checks available disk space — unhealthy if < 500 MB free
    /// </summary>
    public class DiskSpaceHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory)!);
            var freeBytes = drive.AvailableFreeSpace;
            var freeMb = freeBytes / (1024 * 1024);

            if (freeMb < 500)
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Low disk space: {freeMb} MB remaining."));

            if (freeMb < 2000)
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Disk space warning: {freeMb} MB remaining."));

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Disk OK: {freeMb} MB free."));
        }
    }

    /// <summary>
    /// Checks allocated memory — degraded if > 1.5 GB
    /// </summary>
    public class MemoryHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(forceFullCollection: false);
            var allocatedMb = allocated / (1024 * 1024);

            if (allocatedMb > 1500)
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"High memory usage: {allocatedMb} MB allocated."));

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory OK: {allocatedMb} MB allocated."));
        }
    }
}