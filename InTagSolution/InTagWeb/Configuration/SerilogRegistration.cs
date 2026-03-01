using Serilog;
using Serilog.Events;

namespace InTagWeb.Configuration
{
    public static class SerilogRegistration
    {
        public static WebApplicationBuilder AddInTagSerilog(
            this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("Application", "InTag")
                    // Console — all environments
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}" +
                        "  {Message:lj}{NewLine}" +
                        "{Exception}")
                    // File — rolling daily, 30-day retention
                    .WriteTo.File(
                        path: "Logs/intag-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                        "[{SourceContext}] [TenantId:{TenantId}] [UserId:{UserId}] " +
                        "{Message:lj}{NewLine}{Exception}")
                    // Seq — centralized structured logging
                    .WriteTo.Seq(
                        serverUrl: context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
                        apiKey: context.Configuration["Seq:ApiKey"],
                        restrictedToMinimumLevel: LogEventLevel.Information);
            });

            return builder;
        }
    }
}