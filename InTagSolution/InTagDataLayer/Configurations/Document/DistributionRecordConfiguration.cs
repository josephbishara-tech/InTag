using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class DistributionRecordConfiguration : BaseEntityConfiguration<DistributionRecord>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DistributionRecord> builder)
        {
            builder.ToTable("DistributionRecord");

            builder.Property(d => d.RecipientType).IsRequired().HasMaxLength(50);
            builder.Property(d => d.RecipientIdentifier).IsRequired().HasMaxLength(200);
            builder.Property(d => d.RecipientName).HasMaxLength(200);

            builder.HasIndex(d => new { d.DocumentId, d.RecipientIdentifier })
                .HasDatabaseName("IX_DistributionRecord_DocId_Recipient");

            builder.HasOne(d => d.Document)
                .WithMany(doc => doc.DistributionRecords)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
