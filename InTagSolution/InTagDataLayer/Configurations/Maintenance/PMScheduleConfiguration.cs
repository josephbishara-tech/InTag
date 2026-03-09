using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Maintenance;

namespace InTagDataLayer.Configurations.Maintenance
{
    public class PMScheduleConfiguration : BaseEntityConfiguration<PMSchedule>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PMSchedule> builder)
        {
            builder.ToTable("PMSchedule");
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.MeterType).HasMaxLength(100);
            builder.Property(p => p.MeterIntervalValue).HasPrecision(18, 2);
            builder.Property(p => p.LastMeterReading).HasPrecision(18, 2);
            builder.Property(p => p.NextMeterThreshold).HasPrecision(18, 2);
            builder.Property(p => p.EstimatedLaborHours).HasPrecision(10, 2);
            builder.Property(p => p.SLATargetHours).HasPrecision(10, 2);
            builder.Property(p => p.TaskDescription).HasMaxLength(2000);

            builder.HasIndex(p => new { p.AssetId, p.IsEnabled })
                .HasDatabaseName("IX_PMSchedule_AssetId_Enabled");
            builder.HasIndex(p => p.NextDueDate)
                .HasDatabaseName("IX_PMSchedule_NextDueDate");

            builder.HasOne(p => p.Asset).WithMany()
                .HasForeignKey(p => p.AssetId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
