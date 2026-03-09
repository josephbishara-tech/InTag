using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class ProductionOrderConfiguration : BaseEntityConfiguration<ProductionOrder>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProductionOrder> builder)
        {
            builder.ToTable("ProductionOrder");
            builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(o => o.PlannedQuantity).HasPrecision(18, 4);
            builder.Property(o => o.CompletedQuantity).HasPrecision(18, 4);
            builder.Property(o => o.ScrapQuantity).HasPrecision(18, 4);
            builder.Property(o => o.Notes).HasMaxLength(2000);

            builder.HasIndex(o => new { o.TenantId, o.OrderNumber })
                .IsUnique().HasDatabaseName("IX_ProdOrder_TenantId_OrderNumber");
            builder.HasIndex(o => new { o.TenantId, o.Status })
                .HasDatabaseName("IX_ProdOrder_TenantId_Status");

            builder.HasOne(o => o.Product).WithMany()
                .HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.BOM).WithMany()
                .HasForeignKey(o => o.BOMId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(o => o.Routing).WithMany()
                .HasForeignKey(o => o.RoutingId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
