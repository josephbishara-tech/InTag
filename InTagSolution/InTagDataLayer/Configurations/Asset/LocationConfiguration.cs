using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class LocationConfiguration : BaseEntityConfiguration<Location>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("Location");

            builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
            builder.Property(l => l.Code).HasMaxLength(20);
            builder.Property(l => l.Address).HasMaxLength(500);
            builder.Property(l => l.Building).HasMaxLength(100);
            builder.Property(l => l.Floor).HasMaxLength(50);
            builder.Property(l => l.Room).HasMaxLength(50);

            builder.HasIndex(l => new { l.TenantId, l.Code })
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL")
                .HasDatabaseName("IX_Location_TenantId_Code");

            builder.HasOne(l => l.ParentLocation)
                .WithMany(l => l.ChildLocations)
                .HasForeignKey(l => l.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
