using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class DepartmentConfiguration : BaseEntityConfiguration<Department>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Department");

            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Code).HasMaxLength(20);
            builder.Property(d => d.Description).HasMaxLength(500);

            builder.HasIndex(d => new { d.TenantId, d.Name })
                .IsUnique()
                .HasDatabaseName("IX_Department_TenantId_Name");

            builder.HasOne(d => d.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
