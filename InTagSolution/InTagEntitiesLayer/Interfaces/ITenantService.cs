using InTagEntitiesLayer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InTagEntitiesLayer.Interfaces
{
    public interface ITenantService
    {
        Guid GetCurrentTenantId();
        Guid GetCurrentUserId();
        Tenant? GetCurrentTenant();
        void SetCurrentTenant(Tenant tenant);
    }
}
