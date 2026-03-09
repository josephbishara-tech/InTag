using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class RoutingOperationConfiguration : BaseEntityConfiguration<RoutingOperation>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<RoutingOperation> builder)
        {
            builder.ToTable("RoutingOperation");
            builder.Property(o => o.OperationName).IsRequired().HasMaxLength(200);
            builder.Property(o => o.SetupTimeMinutes).HasPrecision(10, 2);
            builder.Property(o => o.RunTimePerUnitMinutes).HasPrecision(10, 4);
            builder.Property(o => o.Instructions).HasMaxLength(1000);

            builder.HasIndex(o => new { o.RoutingId, o.Sequence })
                .IsUnique().HasDatabaseName("IX_RoutingOp_RoutingId_Sequence");

            builder.HasOne(o => o.Routing).WithMany(r => r.Operations)
                .HasForeignKey(o => o.RoutingId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(o => o.WorkCenter).WithMany(w => w.Operations)
                .HasForeignKey(o => o.WorkCenterId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
