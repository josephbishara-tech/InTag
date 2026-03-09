using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class LotBatchConfiguration : BaseEntityConfiguration<LotBatch>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<LotBatch> builder)
        {
            builder.ToTable("LotBatch");
            builder.Property(l => l.LotNumber).IsRequired().HasMaxLength(50);
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.Property(l => l.StorageLocation).HasMaxLength(200);
            builder.Property(l => l.Notes).HasMaxLength(2000);

            builder.HasIndex(l => new { l.TenantId, l.LotNumber })
                .IsUnique().HasDatabaseName("IX_LotBatch_TenantId_LotNumber");

            builder.HasOne(l => l.Product).WithMany()
                .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(l => l.ProductionOrder).WithMany(o => o.LotBatches)
                .HasForeignKey(l => l.ProductionOrderId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
