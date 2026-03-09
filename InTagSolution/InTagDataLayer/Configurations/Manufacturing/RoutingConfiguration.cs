using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Manufacturing;

namespace InTagDataLayer.Configurations.Manufacturing
{
    public class RoutingConfiguration : BaseEntityConfiguration<Routing>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Routing> builder)
        {
            builder.ToTable("Routing");
            builder.Property(r => r.RoutingCode).IsRequired().HasMaxLength(50);
            builder.Property(r => r.Version).IsRequired().HasMaxLength(20);
            builder.Property(r => r.TotalCycleTimeMinutes).HasPrecision(12, 2);
            builder.Property(r => r.Notes).HasMaxLength(1000);

            builder.HasIndex(r => new { r.TenantId, r.RoutingCode })
                .IsUnique().HasDatabaseName("IX_Routing_TenantId_RoutingCode");

            builder.HasOne(r => r.Product).WithMany()
                .HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
