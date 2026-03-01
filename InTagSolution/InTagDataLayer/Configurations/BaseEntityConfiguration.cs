using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Common;

namespace InTagDataLayer.Configurations
{
    public abstract class BaseEntityConfiguration<TEntity>
        : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Primary key
            builder.HasKey(e => e.Id);

            // TenantId - required, indexed
            builder.Property(e => e.TenantId)
                .IsRequired();

            // Audit fields
            builder.Property(e => e.CreatedByUserId)
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .IsRequired();

            // Soft delete
            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Concurrency token
            builder.Property(e => e.RowVersion)
                .IsRowVersion();

            // Indexes per indexing strategy:
            // Non-clustered on TenantId + IsActive
            builder.HasIndex(e => new { e.TenantId, e.IsActive })
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_TenantId_IsActive");

            // Filtered index on IsActive = true
            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsActive")
                .HasFilter("[IsActive] = 1");

            // Apply entity-specific configuration
            ConfigureEntity(builder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
    }
}
