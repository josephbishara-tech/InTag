using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Maintenance;

namespace InTagDataLayer.Configurations.Maintenance
{
    public class WorkOrderPartConfiguration : BaseEntityConfiguration<WorkOrderPart>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkOrderPart> builder)
        {
            builder.ToTable("WorkOrderPart");
            builder.Property(p => p.PartNumber).IsRequired().HasMaxLength(100);
            builder.Property(p => p.PartName).IsRequired().HasMaxLength(300);
            builder.Property(p => p.Quantity).HasPrecision(18, 4);
            builder.Property(p => p.UnitCost).HasPrecision(18, 2);
            builder.Property(p => p.Notes).HasMaxLength(500);

            builder.HasIndex(p => p.WorkOrderId).HasDatabaseName("IX_WOPart_WorkOrderId");

            builder.HasOne(p => p.WorkOrder).WithMany(w => w.Parts)
                .HasForeignKey(p => p.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
