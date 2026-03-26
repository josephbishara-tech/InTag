using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class UserFolderConfiguration : BaseEntityConfiguration<UserFolder>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserFolder> builder)
        {
            builder.ToTable("UserFolder");
            builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
            builder.Property(f => f.Description).HasMaxLength(500);
            builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(1000);

            builder.HasIndex(f => new { f.TenantId, f.OwnerUserId, f.Name, f.ParentFolderId })
                .HasDatabaseName("IX_UserFolder_Owner_Name");

            builder.HasOne(f => f.ParentFolder).WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentFolderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(f => f.Department).WithMany()
                .HasForeignKey(f => f.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
