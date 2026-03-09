using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Workflow;

namespace InTagDataLayer.Configurations.Workflow
{
    public class WorkflowDefinitionConfiguration : BaseEntityConfiguration<WorkflowDefinition>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkflowDefinition> builder)
        {
            builder.ToTable("WorkflowDefinition");
            builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
            builder.Property(w => w.Description).HasMaxLength(1000);
            builder.Property(w => w.Version).IsRequired().HasMaxLength(20);
            builder.Property(w => w.Module).HasMaxLength(50);

            builder.HasIndex(w => new { w.TenantId, w.Name, w.Version })
                .IsUnique().HasDatabaseName("IX_WFDef_TenantId_Name_Version");
            builder.HasIndex(w => new { w.TenantId, w.Category })
                .HasDatabaseName("IX_WFDef_TenantId_Category");
        }
    }
}
