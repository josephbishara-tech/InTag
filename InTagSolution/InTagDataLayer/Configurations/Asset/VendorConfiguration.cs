using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class VendorConfiguration : BaseEntityConfiguration<Vendor>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Vendor> builder)
        {
            builder.ToTable("Vendor");

            builder.Property(v => v.Name).IsRequired().HasMaxLength(100);
            builder.Property(v => v.Code).HasMaxLength(20);
            builder.Property(v => v.ContactPerson).HasMaxLength(200);
            builder.Property(v => v.Email).HasMaxLength(100);
            builder.Property(v => v.Phone).HasMaxLength(30);
            builder.Property(v => v.Address).HasMaxLength(500);
            builder.Property(v => v.Website).HasMaxLength(200);
            builder.Property(v => v.Notes).HasMaxLength(1000);

            builder.HasIndex(v => new { v.TenantId, v.Name })
                .IsUnique()
                .HasDatabaseName("IX_Vendor_TenantId_Name");
        }
    }
}
