using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Maintenance;

namespace InTagDataLayer.Configurations.Maintenance
{
    public class WorkOrderLaborConfiguration : BaseEntityConfiguration<WorkOrderLabor>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkOrderLabor> builder)
        {
            builder.ToTable("WorkOrderLabor");
            builder.Property(l => l.TechnicianName).HasMaxLength(200);
            builder.Property(l => l.HoursWorked).HasPrecision(10, 2);
            builder.Property(l => l.HourlyRate).HasPrecision(10, 2);
            builder.Property(l => l.WorkPerformed).HasMaxLength(1000);

            builder.HasIndex(l => l.WorkOrderId).HasDatabaseName("IX_WOLabor_WorkOrderId");

            builder.HasOne(l => l.WorkOrder).WithMany(w => w.LaborEntries)
                .HasForeignKey(l => l.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
