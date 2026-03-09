using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Product");
            builder.Property(p => p.ProductCode).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.Category).HasMaxLength(100);
            builder.Property(p => p.Barcode).HasMaxLength(100);
            builder.Property(p => p.StandardCost).HasPrecision(18, 2);
            builder.Property(p => p.Notes).HasMaxLength(2000);

            builder.HasIndex(p => new { p.TenantId, p.ProductCode })
                .IsUnique().HasDatabaseName("IX_Product_TenantId_ProductCode");
        }
    }
}
