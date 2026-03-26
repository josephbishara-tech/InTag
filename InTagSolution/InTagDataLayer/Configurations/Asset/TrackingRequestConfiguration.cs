using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class TrackingRequestConfiguration : BaseEntityConfiguration<TrackingRequest>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<TrackingRequest> builder)
        {
            builder.ToTable("TrackingRequest");
            builder.Property(t => t.RequestNumber).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(1000);
            builder.Property(t => t.Notes).HasMaxLength(2000);

            builder.HasIndex(t => new { t.TenantId, t.RequestNumber })
                .IsUnique().HasDatabaseName("IX_TrackReq_TenantId_Number");
            builder.HasIndex(t => t.Status).HasDatabaseName("IX_TrackReq_Status");

            builder.HasOne(t => t.Location).WithMany()
                .HasForeignKey(t => t.LocationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
