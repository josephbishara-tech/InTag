using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Inventory;

namespace InTagDataLayer.Configurations.Inventory
{
    public class InventoryTransactionConfiguration : BaseEntityConfiguration<InventoryTransaction>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransaction");
            builder.Property(t => t.TransactionNumber).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Quantity).HasPrecision(18, 4);
            builder.Property(t => t.UnitCost).HasPrecision(18, 4);
            builder.Property(t => t.ReferenceNumber).HasMaxLength(200);
            builder.Property(t => t.LotNumber).HasMaxLength(50);
            builder.Property(t => t.SerialNumber).HasMaxLength(50);
            builder.Property(t => t.Reason).HasMaxLength(1000);
            builder.Property(t => t.Notes).HasMaxLength(2000);

            builder.HasIndex(t => new { t.TenantId, t.TransactionNumber })
                .IsUnique().HasDatabaseName("IX_InvTxn_TenantId_Number");
            builder.HasIndex(t => new { t.ProductId, t.TransactionDate })
                .HasDatabaseName("IX_InvTxn_ProductId_Date");
            builder.HasIndex(t => t.TransactionDate)
                .HasDatabaseName("IX_InvTxn_Date");

            builder.HasOne(t => t.Product).WithMany()
                .HasForeignKey(t => t.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.Warehouse).WithMany()
                .HasForeignKey(t => t.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.ToWarehouse).WithMany()
                .HasForeignKey(t => t.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
