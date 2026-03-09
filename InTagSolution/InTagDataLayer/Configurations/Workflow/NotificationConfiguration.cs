using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Workflow;

namespace InTagDataLayer.Configurations.Workflow
{
    public class NotificationConfiguration : BaseEntityConfiguration<Notification>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notification");
            builder.Property(n => n.Title).IsRequired().HasMaxLength(300);
            builder.Property(n => n.Message).HasMaxLength(2000);
            builder.Property(n => n.ActionUrl).HasMaxLength(500);

            builder.HasIndex(n => new { n.RecipientUserId, n.IsRead })
                .HasDatabaseName("IX_Notification_Recipient_Read");
            builder.HasIndex(n => n.CreatedDate)
                .HasDatabaseName("IX_Notification_CreatedDate");

            builder.HasOne(n => n.WorkflowInstance).WithMany()
                .HasForeignKey(n => n.WorkflowInstanceId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
