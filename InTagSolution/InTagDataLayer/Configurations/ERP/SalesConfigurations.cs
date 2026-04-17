using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.ERP;

namespace InTagDataLayer.Configurations.ERP
{
    public class CustomerConfiguration : BaseEntityConfiguration<Customer>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer");
            builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.CreditLimit).HasPrecision(18, 2);
            builder.Property(c => c.Currency).HasMaxLength(3);
            builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique().HasDatabaseName("IX_Customer_Code");
            builder.HasOne(c => c.Pricelist).WithMany().HasForeignKey(c => c.PricelistId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(c => c.SalesTeam).WithMany().HasForeignKey(c => c.SalesTeamId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class PricelistConfiguration : BaseEntityConfiguration<Pricelist>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Pricelist> builder)
        {
            builder.ToTable("Pricelist");
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Currency).HasMaxLength(3);
            builder.HasIndex(p => new { p.TenantId, p.Name }).IsUnique().HasDatabaseName("IX_Pricelist_Name");
        }
    }

    public class PricelistLineConfiguration : BaseEntityConfiguration<PricelistLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<PricelistLine> builder)
        {
            builder.ToTable("PricelistLine");
            builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
            builder.Property(l => l.MinQuantity).HasPrecision(18, 4);
            builder.HasOne(l => l.Pricelist).WithMany(p => p.Lines).HasForeignKey(l => l.PricelistId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SalesTeamConfiguration : BaseEntityConfiguration<SalesTeam>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<SalesTeam> builder)
        {
            builder.ToTable("SalesTeam");
            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
            builder.Property(t => t.DefaultCommissionPercent).HasPrecision(5, 2);
        }
    }

    public class CommissionRuleConfiguration : BaseEntityConfiguration<CommissionRule>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CommissionRule> builder)
        {
            builder.ToTable("CommissionRule");
            builder.Property(r => r.CommissionPercent).HasPrecision(5, 2);
            builder.Property(r => r.MinOrderAmount).HasPrecision(18, 2);
            builder.Property(r => r.MaxOrderAmount).HasPrecision(18, 2);
            builder.HasOne(r => r.SalesTeam).WithMany(t => t.CommissionRules).HasForeignKey(r => r.SalesTeamId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class QuotationConfiguration : BaseEntityConfiguration<Quotation>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Quotation> builder)
        {
            builder.ToTable("Quotation");
            builder.Property(q => q.QuotationNumber).IsRequired().HasMaxLength(50);
            builder.Property(q => q.SubTotal).HasPrecision(18, 2);
            builder.Property(q => q.DiscountAmount).HasPrecision(18, 2);
            builder.Property(q => q.TaxAmount).HasPrecision(18, 2);
            builder.Property(q => q.Total).HasPrecision(18, 2);
            builder.HasIndex(q => new { q.TenantId, q.QuotationNumber }).IsUnique().HasDatabaseName("IX_Quotation_Number");
            builder.HasOne(q => q.Customer).WithMany().HasForeignKey(q => q.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(q => q.SalesTeam).WithMany().HasForeignKey(q => q.SalesTeamId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(q => q.SalesOrder).WithMany().HasForeignKey(q => q.SalesOrderId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class QuotationLineConfiguration : BaseEntityConfiguration<QuotationLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<QuotationLine> builder)
        {
            builder.ToTable("QuotationLine");
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
            builder.Property(l => l.DiscountPercent).HasPrecision(5, 2);
            builder.Property(l => l.TaxPercent).HasPrecision(5, 2);
            builder.HasOne(l => l.Quotation).WithMany(q => q.Lines).HasForeignKey(l => l.QuotationId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SalesOrderConfiguration : BaseEntityConfiguration<SalesOrder>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<SalesOrder> builder)
        {
            builder.ToTable("SalesOrder");
            builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(o => o.SubTotal).HasPrecision(18, 2);
            builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
            builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
            builder.Property(o => o.Total).HasPrecision(18, 2);
            builder.Property(o => o.PaidAmount).HasPrecision(18, 2);
            builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique().HasDatabaseName("IX_SalesOrder_Number");
            builder.HasOne(o => o.Customer).WithMany(c => c.SalesOrders).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.SalesTeam).WithMany().HasForeignKey(o => o.SalesTeamId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(o => o.Quotation).WithMany().HasForeignKey(o => o.QuotationId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class SalesOrderLineConfiguration : BaseEntityConfiguration<SalesOrderLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<SalesOrderLine> builder)
        {
            builder.ToTable("SalesOrderLine");
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.Property(l => l.DeliveredQuantity).HasPrecision(18, 4);
            builder.Property(l => l.InvoicedQuantity).HasPrecision(18, 4);
            builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
            builder.Property(l => l.DiscountPercent).HasPrecision(5, 2);
            builder.Property(l => l.TaxPercent).HasPrecision(5, 2);
            builder.HasOne(l => l.SalesOrder).WithMany(o => o.Lines).HasForeignKey(l => l.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
