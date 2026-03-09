using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Workflow;

namespace InTagDataLayer.Configurations.Workflow
{
    public class WorkflowStepConfiguration : BaseEntityConfiguration<WorkflowStep>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkflowStep> builder)
        {
            builder.ToTable("WorkflowStep");
            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).HasMaxLength(1000);
            builder.Property(s => s.AssigneeRole).IsRequired().HasMaxLength(100);
            builder.Property(s => s.EscalationToRole).HasMaxLength(100);
            builder.Property(s => s.ConditionExpression).HasMaxLength(2000);

            builder.HasIndex(s => new { s.WorkflowDefinitionId, s.StepOrder })
                .IsUnique().HasDatabaseName("IX_WFStep_DefId_Order");

            builder.HasOne(s => s.WorkflowDefinition).WithMany(d => d.Steps)
                .HasForeignKey(s => s.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Department).WithMany()
                .HasForeignKey(s => s.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
