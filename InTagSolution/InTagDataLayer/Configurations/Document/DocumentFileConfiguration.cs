using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class DocumentFileConfiguration : BaseEntityConfiguration<DocumentFile>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DocumentFile> builder)
        {
            builder.ToTable("DocumentFile");

            builder.Property(f => f.FileName).IsRequired().HasMaxLength(300);
            builder.Property(f => f.FileType).IsRequired().HasMaxLength(100);
            builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(1000);
            builder.Property(f => f.Hash).HasMaxLength(128);

            builder.HasIndex(f => f.RevisionId)
                .HasDatabaseName("IX_DocumentFile_RevisionId");

            builder.HasOne(f => f.Revision)
                .WithMany(r => r.Files)
                .HasForeignKey(f => f.RevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
