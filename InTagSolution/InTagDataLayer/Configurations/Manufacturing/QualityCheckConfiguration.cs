using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class QualityCheckConfiguration : BaseEntityConfiguration<QualityCheck>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<QualityCheck> builder)
        {
            builder.ToTable("QualityCheck");
            builder.Property(q => q.CheckName).IsRequired().HasMaxLength(200);
            builder.Property(q => q.Specification).HasMaxLength(500);
            builder.Property(q => q.ActualValue).HasMaxLength(200);
            builder.Property(q => q.Findings).HasMaxLength(1000);
            builder.Property(q => q.CorrectiveAction).HasMaxLength(1000);

            builder.HasIndex(q => q.ProductionOrderId)
                .HasDatabaseName("IX_QualityCheck_ProdOrderId");
            builder.HasIndex(q => q.LotBatchId)
                .HasDatabaseName("IX_QualityCheck_LotBatchId");

            builder.HasOne(q => q.ProductionOrder).WithMany(o => o.QualityChecks)
                .HasForeignKey(q => q.ProductionOrderId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(q => q.LotBatch).WithMany(l => l.QualityChecks)
                .HasForeignKey(q => q.LotBatchId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(q => q.RoutingOperation).WithMany()
                .HasForeignKey(q => q.RoutingOperationId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
