using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.Asset;

namespace InTagDataLayer.Configurations.Asset
{
    public class InspectionConfiguration : BaseEntityConfiguration<Inspection>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Inspection> builder)
        {
            builder.ToTable("Inspection");

            builder.Property(i => i.Findings).IsRequired().HasMaxLength(2000);
            builder.Property(i => i.Recommendations).HasMaxLength(2000);
            builder.Property(i => i.ChecklistName).HasMaxLength(200);
            builder.Property(i => i.Notes).HasMaxLength(1000);

            builder.HasIndex(i => new { i.AssetId, i.InspectionDate })
                .HasDatabaseName("IX_Inspection_AssetId_InspectionDate");

            builder.HasIndex(i => i.NextDueDate)
                .HasDatabaseName("IX_Inspection_NextDueDate");

            builder.HasOne(i => i.Asset)
                .WithMany(a => a.Inspections)
                .HasForeignKey(i => i.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
