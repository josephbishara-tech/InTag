using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Workflow;

namespace InTagDataLayer.Configurations.Workflow
{
    public class WorkflowInstanceConfiguration : BaseEntityConfiguration<WorkflowInstance>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkflowInstance> builder)
        {
            builder.ToTable("WorkflowInstance");
            builder.Property(i => i.InstanceNumber).IsRequired().HasMaxLength(50);
            builder.Property(i => i.EntityType).IsRequired().HasMaxLength(100);
            builder.Property(i => i.EntityReference).HasMaxLength(200);
            builder.Property(i => i.Notes).HasMaxLength(2000);

            builder.HasIndex(i => new { i.TenantId, i.InstanceNumber })
                .IsUnique().HasDatabaseName("IX_WFInst_TenantId_Number");
            builder.HasIndex(i => new { i.EntityType, i.EntityId })
                .HasDatabaseName("IX_WFInst_EntityType_EntityId");
            builder.HasIndex(i => i.Status)
                .HasDatabaseName("IX_WFInst_Status");

            builder.HasOne(i => i.WorkflowDefinition).WithMany(d => d.Instances)
                .HasForeignKey(i => i.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
