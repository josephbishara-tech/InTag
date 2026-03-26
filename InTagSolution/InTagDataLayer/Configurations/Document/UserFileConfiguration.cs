using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class UserFileConfiguration : BaseEntityConfiguration<UserFile>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserFile> builder)
        {
            builder.ToTable("UserFile");
            builder.Property(f => f.FileName).IsRequired().HasMaxLength(300);
            builder.Property(f => f.FileType).IsRequired().HasMaxLength(50);
            builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(1000);
            builder.Property(f => f.Hash).HasMaxLength(128);
            builder.Property(f => f.Description).HasMaxLength(500);

            builder.HasIndex(f => new { f.FolderId, f.FileName })
                .HasDatabaseName("IX_UserFile_Folder_Name");

            builder.HasOne(f => f.Folder).WithMany(d => d.Files)
                .HasForeignKey(f => f.FolderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
