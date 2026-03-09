using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Maintenance;

namespace InTagDataLayer.Configurations.Maintenance
{
    public class FailureLogConfiguration : BaseEntityConfiguration<FailureLog>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FailureLog> builder)
        {
            builder.ToTable("FailureLog");
            builder.Property(f => f.DowntimeHours).HasPrecision(10, 2);
            builder.Property(f => f.Description).HasMaxLength(1000);
            builder.Property(f => f.RootCause).HasMaxLength(1000);
            builder.Property(f => f.CorrectiveAction).HasMaxLength(1000);

            builder.HasIndex(f => new { f.AssetId, f.FailureDate })
                .HasDatabaseName("IX_FailureLog_AssetId_Date");

            builder.HasOne(f => f.Asset).WithMany()
                .HasForeignKey(f => f.AssetId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(f => f.WorkOrder).WithMany()
                .HasForeignKey(f => f.WorkOrderId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
