using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class AssetTransferConfiguration : BaseEntityConfiguration<AssetTransfer>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<AssetTransfer> builder)
        {
            builder.ToTable("AssetTransfer");

            builder.Property(t => t.Reason).IsRequired().HasMaxLength(500);
            builder.Property(t => t.Notes).HasMaxLength(1000);

            builder.HasIndex(t => new { t.TenantId, t.TransferDate })
                .HasDatabaseName("IX_AssetTransfer_TenantId_TransferDate");

            builder.HasOne(t => t.Asset)
                .WithMany(a => a.TransfersFrom)
                .HasForeignKey(t => t.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.FromLocation)
                .WithMany()
                .HasForeignKey(t => t.FromLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ToLocation)
                .WithMany()
                .HasForeignKey(t => t.ToLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
