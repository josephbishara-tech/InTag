using System;
using System.Collections.Generic;
using System.Text;

namespace InTagEntitiesLayer.Common
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        Guid TenantId { get; set; }
        Guid CreatedByUserId { get; set; }
        DateTimeOffset CreatedDate { get; set; }
        Guid? ModifiedByUserId { get; set; }
        DateTimeOffset? ModifiedDate { get; set; }
        bool IsActive { get; set; }
        byte[] RowVersion { get; set; }
    }
}
