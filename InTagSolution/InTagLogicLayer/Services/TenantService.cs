using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Interfaces;



namespace InTagLogicLayer.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Tenant? _currentTenant;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentTenantId()
        {
            // First try the resolved tenant from middleware
            if (_currentTenant != null)
                return _currentTenant.Id;

            // Fallback: read from JWT claim
            var tenantClaim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue("tenant_id");

            if (Guid.TryParse(tenantClaim, out var tenantId))
                return tenantId;

            throw new UnauthorizedAccessException("Tenant could not be resolved.");
        }

        public Guid GetCurrentUserId()
        {
            var userClaim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userClaim, out var userId))
                return userId;

            return Guid.Empty; // Anonymous / system operations
        }

        public Tenant? GetCurrentTenant() => _currentTenant;

        public void SetCurrentTenant(Tenant tenant)
        {
            _currentTenant = tenant;
        }
    }
}

