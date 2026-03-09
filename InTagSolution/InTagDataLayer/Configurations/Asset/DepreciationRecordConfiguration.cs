using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class DepreciationRecordConfiguration : BaseEntityConfiguration<DepreciationRecord>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DepreciationRecord> builder)
        {
            builder.ToTable("DepreciationRecord");

            builder.Property(d => d.Period).IsRequired().HasMaxLength(7);
            builder.Property(d => d.OpeningBookValue).HasPrecision(18, 2);
            builder.Property(d => d.DepreciationAmount).HasPrecision(18, 2);
            builder.Property(d => d.AccumulatedDepreciation).HasPrecision(18, 2);
            builder.Property(d => d.ClosingBookValue).HasPrecision(18, 2);
            builder.Property(d => d.UnitsProduced).HasPrecision(18, 4);

            // One depreciation per asset per period
            builder.HasIndex(d => new { d.AssetId, d.Period })
                .IsUnique()
                .HasDatabaseName("IX_DepreciationRecord_AssetId_Period");

            builder.HasOne(d => d.Asset)
                .WithMany(a => a.DepreciationRecords)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
