using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class WorkCenterConfiguration : BaseEntityConfiguration<WorkCenter>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkCenter> builder)
        {
            builder.ToTable("WorkCenter");
            builder.Property(w => w.Code).IsRequired().HasMaxLength(50);
            builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
            builder.Property(w => w.Description).HasMaxLength(500);
            builder.Property(w => w.CapacityHoursPerDay).HasPrecision(8, 2);
            builder.Property(w => w.CostPerHour).HasPrecision(18, 2);

            builder.HasIndex(w => new { w.TenantId, w.Code })
                .IsUnique().HasDatabaseName("IX_WorkCenter_TenantId_Code");

            builder.HasOne(w => w.Location).WithMany()
                .HasForeignKey(w => w.LocationId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(w => w.Department).WithMany()
                .HasForeignKey(w => w.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
