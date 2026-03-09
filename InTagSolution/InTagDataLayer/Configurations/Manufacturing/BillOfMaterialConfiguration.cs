using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class BillOfMaterialConfiguration : BaseEntityConfiguration<BillOfMaterial>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BillOfMaterial> builder)
        {
            builder.ToTable("BillOfMaterial");
            builder.Property(b => b.BOMCode).IsRequired().HasMaxLength(50);
            builder.Property(b => b.Version).IsRequired().HasMaxLength(20);
            builder.Property(b => b.OutputQuantity).HasPrecision(18, 4);
            builder.Property(b => b.Notes).HasMaxLength(1000);

            builder.HasIndex(b => new { b.TenantId, b.BOMCode })
                .IsUnique().HasDatabaseName("IX_BOM_TenantId_BOMCode");

            builder.HasOne(b => b.Product).WithMany(p => p.BOMs)
                .HasForeignKey(b => b.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
