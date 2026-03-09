using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class ProductionLogConfiguration : BaseEntityConfiguration<ProductionLog>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ProductionLog> builder)
        {
            builder.ToTable("ProductionLog");
            builder.Property(l => l.QuantityProduced).HasPrecision(18, 4);
            builder.Property(l => l.QuantityScrapped).HasPrecision(18, 4);
            builder.Property(l => l.QuantityRework).HasPrecision(18, 4);
            builder.Property(l => l.SetupTimeActual).HasPrecision(10, 2);
            builder.Property(l => l.RunTimeActual).HasPrecision(10, 2);
            builder.Property(l => l.DowntimeMinutes).HasPrecision(10, 2);
            builder.Property(l => l.Notes).HasMaxLength(1000);

            builder.HasIndex(l => new { l.ProductionOrderId, l.LogDate })
                .HasDatabaseName("IX_ProdLog_OrderId_LogDate");

            builder.HasOne(l => l.ProductionOrder).WithMany(o => o.ProductionLogs)
                .HasForeignKey(l => l.ProductionOrderId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.RoutingOperation).WithMany()
                .HasForeignKey(l => l.RoutingOperationId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(l => l.WorkCenter).WithMany()
                .HasForeignKey(l => l.WorkCenterId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
