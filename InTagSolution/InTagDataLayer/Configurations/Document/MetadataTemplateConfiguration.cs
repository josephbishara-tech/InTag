using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class MetadataTemplateConfiguration : BaseEntityConfiguration<MetadataTemplate>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<MetadataTemplate> builder)
        {
            builder.ToTable("MetadataTemplate");
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(500);
            builder.HasIndex(t => new { t.TenantId, t.Name }).IsUnique()
                .HasDatabaseName("IX_MetaTemplate_TenantName");
        }
    }

    public class MetadataFieldDefinitionConfiguration : BaseEntityConfiguration<MetadataFieldDefinition>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<MetadataFieldDefinition> builder)
        {
            builder.ToTable("MetadataFieldDefinition");
            builder.Property(f => f.FieldName).IsRequired().HasMaxLength(100);
            builder.Property(f => f.DisplayLabel).HasMaxLength(300);
            builder.Property(f => f.Options).HasMaxLength(2000);
            builder.Property(f => f.DefaultValue).HasMaxLength(500);
            builder.Property(f => f.Placeholder).HasMaxLength(200);
            builder.Property(f => f.HelpText).HasMaxLength(500);

            builder.HasIndex(f => new { f.TemplateId, f.SortOrder })
                .HasDatabaseName("IX_MetaField_Template_Sort");

            builder.HasOne(f => f.Template).WithMany(t => t.Fields)
                .HasForeignKey(f => f.TemplateId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class DocumentMetadataConfiguration : BaseEntityConfiguration<DocumentMetadata>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DocumentMetadata> builder)
        {
            builder.ToTable("DocumentMetadata");
            builder.Property(m => m.Value).HasMaxLength(4000);

            builder.HasIndex(m => new { m.DocumentId, m.FieldDefinitionId })
                .HasDatabaseName("IX_DocMeta_DocId_FieldId");
            builder.HasIndex(m => new { m.UserFileId, m.FieldDefinitionId })
                .HasDatabaseName("IX_DocMeta_FileId_FieldId");

            builder.HasOne(m => m.Document).WithMany()
                .HasForeignKey(m => m.DocumentId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(m => m.UserFile).WithMany()
                .HasForeignKey(m => m.UserFileId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(m => m.UserFolder).WithMany()
                .HasForeignKey(m => m.UserFolderId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(m => m.FieldDefinition).WithMany()
                .HasForeignKey(m => m.FieldDefinitionId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DocumentTagConfiguration : BaseEntityConfiguration<DocumentTag>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DocumentTag> builder)
        {
            builder.ToTable("DocumentTag");
            builder.Property(t => t.Tag).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Color).HasMaxLength(50);

            builder.HasIndex(t => new { t.DocumentId, t.Tag })
                .HasDatabaseName("IX_DocTag_DocId_Tag");
            builder.HasIndex(t => t.Tag)
                .HasDatabaseName("IX_DocTag_Tag");

            builder.HasOne(t => t.Document).WithMany()
                .HasForeignKey(t => t.DocumentId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(t => t.UserFile).WithMany()
                .HasForeignKey(t => t.UserFileId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(t => t.UserFolder).WithMany()
                .HasForeignKey(t => t.UserFolderId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class DocumentLinkConfiguration : BaseEntityConfiguration<DocumentLink>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DocumentLink> builder)
        {
            builder.ToTable("DocumentLink");
            builder.Property(l => l.Description).HasMaxLength(500);

            builder.HasIndex(l => new { l.SourceDocumentId, l.TargetDocumentId, l.LinkType })
                .IsUnique().HasDatabaseName("IX_DocLink_Src_Tgt_Type");

            builder.HasOne(l => l.SourceDocument).WithMany()
                .HasForeignKey(l => l.SourceDocumentId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(l => l.TargetDocument).WithMany()
                .HasForeignKey(l => l.TargetDocumentId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
