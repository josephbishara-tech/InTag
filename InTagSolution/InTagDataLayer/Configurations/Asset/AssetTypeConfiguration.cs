using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class AssetTypeConfiguration : BaseEntityConfiguration<AssetType>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<AssetType> builder)
        {
            builder.ToTable("AssetType");

            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Description).HasMaxLength(500);
            builder.Property(t => t.DefaultSalvageValuePercent).HasPrecision(5, 2);

            builder.HasIndex(t => new { t.TenantId, t.Name })
                .IsUnique()
                .HasDatabaseName("IX_AssetType_TenantId_Name");
        }
    }
}
