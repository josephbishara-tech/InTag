using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class TrackingLineConfiguration : BaseEntityConfiguration<TrackingLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<TrackingLine> builder)
        {
            builder.ToTable("TrackingLine");
            builder.Property(l => l.ScannedCode).HasMaxLength(200);
            builder.Property(l => l.Remarks).HasMaxLength(1000);

            builder.HasIndex(l => new { l.TrackingRequestId, l.AssetId })
                .IsUnique().HasDatabaseName("IX_TrackLine_ReqId_AssetId");

            builder.HasOne(l => l.TrackingRequest).WithMany(r => r.Lines)
                .HasForeignKey(l => l.TrackingRequestId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.Asset).WithMany()
                .HasForeignKey(l => l.AssetId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(l => l.FoundAtLocation).WithMany()
                .HasForeignKey(l => l.FoundAtLocationId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
