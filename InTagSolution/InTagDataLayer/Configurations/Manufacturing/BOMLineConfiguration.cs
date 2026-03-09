using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class BOMLineConfiguration : BaseEntityConfiguration<BOMLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BOMLine> builder)
        {
            builder.ToTable("BOMLine");
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.Property(l => l.ScrapFactor).HasPrecision(8, 2);
            builder.Property(l => l.Notes).HasMaxLength(500);

            builder.HasIndex(l => new { l.BOMId, l.ComponentProductId })
                .HasDatabaseName("IX_BOMLine_BOMId_ComponentId");

            builder.HasOne(l => l.BOM).WithMany(b => b.Lines)
                .HasForeignKey(l => l.BOMId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.ComponentProduct).WithMany()
                .HasForeignKey(l => l.ComponentProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
