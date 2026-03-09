using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Inventory;

namespace InTagDataLayer.Configurations.Inventory
{
    public class StorageBinConfiguration : BaseEntityConfiguration<StorageBin>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StorageBin> builder)
        {
            builder.ToTable("StorageBin");
            builder.Property(b => b.BinCode).IsRequired().HasMaxLength(50);
            builder.Property(b => b.Description).HasMaxLength(200);
            builder.Property(b => b.Aisle).HasMaxLength(50);
            builder.Property(b => b.Shelf).HasMaxLength(50);
            builder.Property(b => b.Level).HasMaxLength(50);
            builder.Property(b => b.MaxCapacity).HasPrecision(18, 4);

            builder.HasIndex(b => new { b.WarehouseId, b.BinCode })
                .IsUnique().HasDatabaseName("IX_StorageBin_WarehouseId_BinCode");

            builder.HasOne(b => b.Warehouse).WithMany(w => w.Bins)
                .HasForeignKey(b => b.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
