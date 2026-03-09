using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class AssetItemConfiguration : BaseEntityConfiguration<AssetItem>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<AssetItem> builder)
        {
            builder.ToTable("Asset");

            builder.Property(a => a.AssetCode).IsRequired().HasMaxLength(50);
            builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Description).HasMaxLength(1000);
            builder.Property(a => a.PurchaseCost).HasPrecision(18, 2);
            builder.Property(a => a.SalvageValue).HasPrecision(18, 2);
            builder.Property(a => a.CurrentBookValue).HasPrecision(18, 2);
            builder.Property(a => a.AccumulatedDepreciation).HasPrecision(18, 2);
            builder.Property(a => a.PurchaseOrderNumber).HasMaxLength(100);
            builder.Property(a => a.SerialNumber).HasMaxLength(100);
            builder.Property(a => a.Barcode).HasMaxLength(100);
            builder.Property(a => a.QrCodeData).HasMaxLength(200);
            builder.Property(a => a.Manufacturer).HasMaxLength(100);
            builder.Property(a => a.ModelNumber).HasMaxLength(100);
            builder.Property(a => a.WarrantyTerms).HasMaxLength(200);
            builder.Property(a => a.Notes).HasMaxLength(2000);

            // Unique asset code per tenant
            builder.HasIndex(a => new { a.TenantId, a.AssetCode })
                .IsUnique()
                .HasDatabaseName("IX_Asset_TenantId_AssetCode");

            // Frequently filtered
            builder.HasIndex(a => new { a.TenantId, a.Status })
                .HasDatabaseName("IX_Asset_TenantId_Status");

            builder.HasIndex(a => new { a.TenantId, a.Category })
                .HasDatabaseName("IX_Asset_TenantId_Category");

            // Self-referencing hierarchy
            builder.HasOne(a => a.ParentAsset)
                .WithMany(a => a.ChildAssets)
                .HasForeignKey(a => a.ParentAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            builder.HasOne(a => a.AssetType)
                .WithMany(t => t.Assets)
                .HasForeignKey(a => a.AssetTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Location)
                .WithMany(l => l.Assets)
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Department)
                .WithMany(d => d.Assets)
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Vendor)
                .WithMany(v => v.Assets)
                .HasForeignKey(a => a.VendorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
