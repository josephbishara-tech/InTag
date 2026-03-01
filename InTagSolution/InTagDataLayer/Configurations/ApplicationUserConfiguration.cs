using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Common;

namespace InTagDataLayer.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.TenantId).IsRequired();
            builder.Property(u => u.FirstName).HasMaxLength(100);
            builder.Property(u => u.LastName).HasMaxLength(100);
            builder.Property(u => u.IsActive).HasDefaultValue(true);

            builder.HasIndex(u => u.TenantId)
                .HasDatabaseName("IX_ApplicationUser_TenantId");
        }
    }
}