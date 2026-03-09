using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Inventory;

namespace InTagDataLayer.Configurations.Inventory
{
    public class CycleCountConfiguration : BaseEntityConfiguration<CycleCount>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CycleCount> builder)
        {
            builder.ToTable("CycleCount");
            builder.Property(c => c.CountNumber).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Notes).HasMaxLength(1000);

            builder.HasIndex(c => new { c.TenantId, c.CountNumber })
                .IsUnique().HasDatabaseName("IX_CycleCount_TenantId_Number");

            builder.HasOne(c => c.Warehouse).WithMany()
                .HasForeignKey(c => c.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
