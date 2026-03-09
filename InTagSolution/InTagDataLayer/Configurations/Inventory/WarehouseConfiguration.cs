using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Inventory;

namespace InTagDataLayer.Configurations.Inventory
{
    public class WarehouseConfiguration : BaseEntityConfiguration<Warehouse>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouse");
            builder.Property(w => w.Code).IsRequired().HasMaxLength(50);
            builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
            builder.Property(w => w.Address).HasMaxLength(500);

            builder.HasIndex(w => new { w.TenantId, w.Code })
                .IsUnique().HasDatabaseName("IX_Warehouse_TenantId_Code");

            builder.HasOne(w => w.Location).WithMany()
                .HasForeignKey(w => w.LocationId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
