using System.Net;
using System.Text.Json;

namespace InTagWeb.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var tenantId = context.Items["TenantId"] as Guid?;
            var traceId = context.TraceIdentifier;

            // Structured log with tenant context
            _logger.LogError(exception,
                "Unhandled exception | TraceId: {TraceId} | TenantId: {TenantId} | Path: {Path} | Method: {Method}",
                traceId, tenantId, context.Request.Path, context.Request.Method);

            // Determine status code based on exception type
            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Authentication required."),
                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            // API requests get JSON; MVC requests get redirected to error page
            if (IsApiRequest(context))
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    errors = new[] { message },
                    traceId,
                    // Only include details in development
                    detail = _env.IsDevelopment() ? exception.ToString() : null
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    }));
            }
            else
            {
                // MVC — redirect to error page
                context.Response.Redirect($"/Home/Error?traceId={traceId}");
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api")
                   || context.Request.Headers.Accept.ToString().Contains("application/json");
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}