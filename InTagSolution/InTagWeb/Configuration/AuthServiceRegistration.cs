using InTagDataLayer.Context;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Interfaces;
using InTagLogicLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InTagWeb.Configuration
{
    public static class AuthServiceRegistration
    {
        public static IServiceCollection AddInTagAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind JWT settings
            var jwtSettings = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettings);

            // ASP.NET Core Identity
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<InTagDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            var secretKey = jwtSettings["SecretKey"]!;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // No tolerance for expiration
                };
            });

            // Register auth service
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        /// <summary>
        /// Seeds the four RBAC roles: Admin, Manager, Operator, Viewer
        /// </summary>
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles = { "Admin", "Manager", "Operator", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }
    }
}