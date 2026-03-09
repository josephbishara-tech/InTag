using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class DocumentConfiguration : BaseEntityConfiguration<InTagEntitiesLayer.Document.Document>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<InTagEntitiesLayer.Document.Document> builder)
        {
            builder.ToTable("Document");

            builder.Property(d => d.DocNumber).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Title).IsRequired().HasMaxLength(300);
            builder.Property(d => d.Description).HasMaxLength(1000);
            builder.Property(d => d.CurrentVersion).IsRequired().HasMaxLength(20);
            builder.Property(d => d.IsoReference).HasMaxLength(200);
            builder.Property(d => d.ConfidentialityLevel).HasMaxLength(100);
            builder.Property(d => d.Tags).HasMaxLength(500);
            builder.Property(d => d.Notes).HasMaxLength(2000);

            // Unique doc number per tenant
            builder.HasIndex(d => new { d.TenantId, d.DocNumber })
                .IsUnique()
                .HasDatabaseName("IX_Document_TenantId_DocNumber");

            builder.HasIndex(d => new { d.TenantId, d.Status })
                .HasDatabaseName("IX_Document_TenantId_Status");

            builder.HasIndex(d => new { d.TenantId, d.Type })
                .HasDatabaseName("IX_Document_TenantId_Type");

            builder.HasIndex(d => d.NextReviewDate)
                .HasDatabaseName("IX_Document_NextReviewDate");

            builder.HasOne(d => d.Department)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
