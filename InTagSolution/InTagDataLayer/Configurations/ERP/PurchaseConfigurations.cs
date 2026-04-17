using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.ERP;

namespace InTagDataLayer.Configurations.ERP
{
    public class RfqConfiguration : BaseEntityConfiguration<Rfq>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Rfq> builder)
        {
            builder.ToTable("Rfq");
            builder.Property(r => r.RfqNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(r => new { r.TenantId, r.RfqNumber }).IsUnique().HasDatabaseName("IX_Rfq_Number");
            builder.HasOne(r => r.Vendor).WithMany().HasForeignKey(r => r.VendorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(r => r.PurchaseOrder).WithMany().HasForeignKey(r => r.PurchaseOrderId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class RfqLineConfiguration : BaseEntityConfiguration<RfqLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<RfqLine> builder)
        {
            builder.ToTable("RfqLine");
            builder.Property(l => l.RequestedQuantity).HasPrecision(18, 4);
            builder.Property(l => l.QuotedUnitPrice).HasPrecision(18, 4);
            builder.HasOne(l => l.Rfq).WithMany(r => r.Lines).HasForeignKey(l => l.RfqId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PurchaseOrderConfiguration : BaseEntityConfiguration<PurchaseOrder>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("PurchaseOrder");
            builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(o => o.SubTotal).HasPrecision(18, 2);
            builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
            builder.Property(o => o.Total).HasPrecision(18, 2);
            builder.Property(o => o.PaidAmount).HasPrecision(18, 2);
            builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique().HasDatabaseName("IX_PurchaseOrder_Number");
            builder.HasOne(o => o.Vendor).WithMany().HasForeignKey(o => o.VendorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.Rfq).WithMany().HasForeignKey(o => o.RfqId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class PurchaseOrderLineConfiguration : BaseEntityConfiguration<PurchaseOrderLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PurchaseOrderLine> builder)
        {
            builder.ToTable("PurchaseOrderLine");
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.Property(l => l.ReceivedQuantity).HasPrecision(18, 4);
            builder.Property(l => l.BilledQuantity).HasPrecision(18, 4);
            builder.Property(l => l.UnitCost).HasPrecision(18, 4);
            builder.Property(l => l.TaxPercent).HasPrecision(5, 2);
            builder.HasOne(l => l.PurchaseOrder).WithMany(o => o.Lines).HasForeignKey(l => l.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
