using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Document;

namespace InTagDataLayer.Configurations.Document
{
    public class ApprovalMatrixConfiguration : BaseEntityConfiguration<ApprovalMatrix>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<ApprovalMatrix> builder)
        {
            builder.ToTable("ApprovalMatrix");

            builder.Property(a => a.ApproverRole).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Description).HasMaxLength(500);

            builder.HasIndex(a => new { a.TenantId, a.DocumentType, a.DepartmentId, a.ApproverLevel })
                .IsUnique()
                .HasDatabaseName("IX_ApprovalMatrix_TenantId_Type_Dept_Level");

            builder.HasOne(a => a.Department)
                .WithMany()
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
