using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Maintenance;

namespace InTagDataLayer.Configurations.Maintenance
{
    public class WorkOrderConfiguration : BaseEntityConfiguration<WorkOrder>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrder");
            builder.Property(w => w.WorkOrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(w => w.Title).IsRequired().HasMaxLength(300);
            builder.Property(w => w.Description).HasMaxLength(2000);
            builder.Property(w => w.FailureCause).HasMaxLength(1000);
            builder.Property(w => w.Resolution).HasMaxLength(2000);
            builder.Property(w => w.Notes).HasMaxLength(2000);
            builder.Property(w => w.SLATargetHours).HasPrecision(10, 2);
            builder.Property(w => w.LaborCost).HasPrecision(18, 2);
            builder.Property(w => w.PartsCost).HasPrecision(18, 2);
            builder.Property(w => w.ExternalCost).HasPrecision(18, 2);

            builder.HasIndex(w => new { w.TenantId, w.WorkOrderNumber })
                .IsUnique().HasDatabaseName("IX_WorkOrder_TenantId_Number");
            builder.HasIndex(w => new { w.TenantId, w.Status })
                .HasDatabaseName("IX_WorkOrder_TenantId_Status");
            builder.HasIndex(w => new { w.AssetId, w.Status })
                .HasDatabaseName("IX_WorkOrder_AssetId_Status");
            builder.HasIndex(w => w.DueDate)
                .HasDatabaseName("IX_WorkOrder_DueDate");

            builder.HasOne(w => w.Asset).WithMany()
                .HasForeignKey(w => w.AssetId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(w => w.PMSchedule).WithMany(p => p.GeneratedWorkOrders)
                .HasForeignKey(w => w.PMScheduleId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
