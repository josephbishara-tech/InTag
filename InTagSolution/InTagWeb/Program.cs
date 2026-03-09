using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Services;
using InTagWeb.Configuration;
using InTagWeb.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// ── Serilog ──────────────────────────────
builder.AddInTagSerilog();

// ── Services ─────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddInTagMultiTenancy(builder.Configuration);
builder.Services.AddInTagAuthentication(builder.Configuration);
builder.Services.AddInTagHealthChecks(builder.Configuration);
builder.Services.AddInTagRepositories();
builder.Services.AddInTagAssetServices();
builder.Services.AddScoped<IWorkflowHook, WorkflowHookService>();
builder.Services.AddInTagDocumentServices(builder.Configuration);
builder.Services.AddInTagManufacturingServices();
builder.Services.AddInTagMaintenanceServices();
builder.Services.AddInTagInventoryServices();




// ── DbContext (main tenant DB) ───────────
builder.Services.AddDbContext<InTagDataLayer.Context.InTagDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ── Seed roles ───────────────────────────
using (var scope = app.Services.CreateScope())
{
    await AuthServiceRegistration.SeedRolesAsync(scope.ServiceProvider);
    await TenantServiceRegistration.SeedDefaultTenantAsync(scope.ServiceProvider);
}

// ═══ MIDDLEWARE PIPELINE (ORDER MATTERS!) ═══

// 1. Global exception handler — catches everything below
app.UseGlobalExceptionHandler();

// 2. Serilog request logging — logs every HTTP request
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// 3. Static files (CSS, JS, images)
app.UseStaticFiles();

// 4. Routing
app.UseRouting();

// 5. Tenant resolution — resolve from subdomain/custom domain
app.UseTenantResolution();

// 6. Log context enrichment — adds TenantId/UserId to all logs
app.UseLogContextEnrichment();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();


// 8. Health check endpoints
app.MapInTagHealthChecks();

// 9. MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
