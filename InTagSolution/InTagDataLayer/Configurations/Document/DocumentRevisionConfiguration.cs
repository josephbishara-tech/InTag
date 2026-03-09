using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class DocumentRevisionConfiguration : BaseEntityConfiguration<DocumentRevision>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DocumentRevision> builder)
        {
            builder.ToTable("DocumentRevision");

            builder.Property(r => r.RevisionNumber).IsRequired().HasMaxLength(20);
            builder.Property(r => r.ChangeDescription).IsRequired().HasMaxLength(1000);
            builder.Property(r => r.ReviewComments).HasMaxLength(1000);
            builder.Property(r => r.ApprovalComments).HasMaxLength(1000);
            builder.Property(r => r.DigitalSignatureData).HasMaxLength(500);
            builder.Property(r => r.Notes).HasMaxLength(2000);

            builder.HasIndex(r => new { r.DocumentId, r.RevisionNumber })
                .IsUnique()
                .HasDatabaseName("IX_DocumentRevision_DocumentId_RevisionNumber");

            builder.HasOne(r => r.Document)
                .WithMany(d => d.Revisions)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
