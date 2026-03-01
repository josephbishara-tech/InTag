using InTagDataLayer.Context;
using InTagEntitiesLayer.Common;
using InTagLogicLayer.Interfaces;
using InTagViewModelLayer.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InTagLogicLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InTagDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            InTagDbContext context,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseVm> LoginAsync(LoginRequestVm request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid credentials.");

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseVm> RegisterAsync(RegisterRequestVm request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = request.TenantId,
                IsActive = true,
                CreatedDate = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            // Assign default Viewer role
            await _userManager.AddToRoleAsync(user, "Viewer");

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseVm> RefreshTokenAsync(RefreshTokenRequestVm request)
        {
            // Validate the expired access token to extract claims
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Find the active refresh token
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken
                                          && r.UserId == userId);

            if (storedToken == null || !storedToken.IsActive)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            // Revoke the old refresh token
            storedToken.RevokedDate = DateTimeOffset.UtcNow;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("User not found or inactive.");

            var response = await GenerateAuthResponseAsync(user);

            // Mark old token as replaced
            storedToken.ReplacedByToken = response.RefreshToken;
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);

            if (storedToken == null || !storedToken.IsActive)
                throw new InvalidOperationException("Token not found or already revoked.");

            storedToken.RevokedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ── Private helpers ──────────────────────────────────────

        private async Task<AuthResponseVm> GenerateAuthResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user);

            return new AuthResponseVm
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTimeOffset.UtcNow
                    .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = new UserInfoVm
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TenantId = user.TenantId,
                    Roles = roles.ToList()
                }
            };
        }

        private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new("tenant_id", user.TenantId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                Token = tokenValue,
                UserId = user.Id,
                TenantId = user.TenantId,
                CreatedDate = DateTimeOffset.UtcNow,
                ExpiresDate = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return tokenValue;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // Allow expired tokens
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }
    }
}
