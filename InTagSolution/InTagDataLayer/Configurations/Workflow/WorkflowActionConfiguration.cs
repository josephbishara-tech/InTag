using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Workflow;

namespace InTagDataLayer.Configurations.Workflow
{
    public class WorkflowActionConfiguration : BaseEntityConfiguration<WorkflowAction>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkflowAction> builder)
        {
            builder.ToTable("WorkflowAction");
            builder.Property(a => a.ActionByUserName).HasMaxLength(200);
            builder.Property(a => a.Comments).HasMaxLength(2000);
            builder.Property(a => a.DelegatedToUserName).HasMaxLength(200);
            builder.Property(a => a.SignatureData).HasMaxLength(500);

            builder.HasIndex(a => a.WorkflowInstanceId)
                .HasDatabaseName("IX_WFAction_InstanceId");

            builder.HasOne(a => a.WorkflowInstance).WithMany(i => i.Actions)
                .HasForeignKey(a => a.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.WorkflowStep).WithMany()
                .HasForeignKey(a => a.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
