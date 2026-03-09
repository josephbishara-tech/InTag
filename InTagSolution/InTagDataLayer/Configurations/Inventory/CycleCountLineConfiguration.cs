using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Inventory;

namespace InTagDataLayer.Configurations.Inventory
{
    public class CycleCountLineConfiguration : BaseEntityConfiguration<CycleCountLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CycleCountLine> builder)
        {
            builder.ToTable("CycleCountLine");
            builder.Property(l => l.SystemQuantity).HasPrecision(18, 4);
            builder.Property(l => l.CountedQuantity).HasPrecision(18, 4);
            builder.Property(l => l.Notes).HasMaxLength(500);

            builder.HasIndex(l => new { l.CycleCountId, l.ProductId })
                .HasDatabaseName("IX_CycleCountLine_CountId_ProductId");

            builder.HasOne(l => l.CycleCount).WithMany(c => c.Lines)
                .HasForeignKey(l => l.CycleCountId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.Product).WithMany()
                .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(l => l.StorageBin).WithMany()
                .HasForeignKey(l => l.StorageBinId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
