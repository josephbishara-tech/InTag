using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Inventory;

namespace InTagDataLayer.Configurations.Inventory
{
    public class StockItemConfiguration : BaseEntityConfiguration<StockItem>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockItem> builder)
        {
            builder.ToTable("StockItem");
            builder.Property(s => s.QuantityOnHand).HasPrecision(18, 4);
            builder.Property(s => s.QuantityReserved).HasPrecision(18, 4);
            builder.Property(s => s.MinimumLevel).HasPrecision(18, 4);
            builder.Property(s => s.MaximumLevel).HasPrecision(18, 4);
            builder.Property(s => s.ReorderPoint).HasPrecision(18, 4);
            builder.Property(s => s.ReorderQuantity).HasPrecision(18, 4);
            builder.Property(s => s.EOQ).HasPrecision(18, 4);
            builder.Property(s => s.UnitCost).HasPrecision(18, 4);
            builder.Property(s => s.LotNumber).HasMaxLength(50);
            builder.Property(s => s.SerialNumber).HasMaxLength(50);

            builder.HasIndex(s => new { s.ProductId, s.WarehouseId, s.StorageBinId, s.LotNumber })
                .HasDatabaseName("IX_StockItem_Product_WH_Bin_Lot");
            builder.HasIndex(s => new { s.TenantId, s.WarehouseId })
                .HasDatabaseName("IX_StockItem_TenantId_Warehouse");

            builder.HasOne(s => s.Product).WithMany()
                .HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(s => s.Warehouse).WithMany()
                .HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(s => s.StorageBin).WithMany()
                .HasForeignKey(s => s.StorageBinId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
