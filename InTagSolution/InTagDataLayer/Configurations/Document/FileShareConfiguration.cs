using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class FileShareConfiguration : BaseEntityConfiguration<InTagEntitiesLayer.Document.FileShare>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<InTagEntitiesLayer.Document.FileShare> builder)
        {
            builder.ToTable("FileShare");
            builder.Property(s => s.Message).HasMaxLength(500);

            builder.HasIndex(s => new { s.UserFileId, s.TargetUserId })
                .HasDatabaseName("IX_FileShare_File_User");
            builder.HasIndex(s => s.TargetUserId)
                .HasDatabaseName("IX_FileShare_TargetUser");

            builder.HasOne(s => s.UserFile).WithMany(f => f.Shares)
                .HasForeignKey(s => s.UserFileId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.TargetDepartment).WithMany()
                .HasForeignKey(s => s.TargetDepartmentId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
