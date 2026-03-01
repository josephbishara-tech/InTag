using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InTagEntitiesLayer.Common
{
    public abstract class BaseEntity : IBaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Required]
        public DateTimeOffset CreatedDate { get; set; }

        public Guid? ModifiedByUserId { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
