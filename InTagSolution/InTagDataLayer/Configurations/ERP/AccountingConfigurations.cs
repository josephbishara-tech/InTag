using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InTagEntitiesLayer.ERP;

namespace InTagDataLayer.Configurations.ERP
{
    public class AccountConfiguration : BaseEntityConfiguration<Account>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Account");
            builder.Property(a => a.AccountCode).IsRequired().HasMaxLength(20);
            builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
            builder.HasIndex(a => new { a.TenantId, a.AccountCode }).IsUnique().HasDatabaseName("IX_Account_Code");
            builder.HasOne(a => a.ParentAccount).WithMany(a => a.ChildAccounts)
                .HasForeignKey(a => a.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CostCenterConfiguration : BaseEntityConfiguration<CostCenter>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CostCenter> builder)
        {
            builder.ToTable("CostCenter");
            builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique().HasDatabaseName("IX_CostCenter_Code");
            builder.HasOne(c => c.ParentCostCenter).WithMany().HasForeignKey(c => c.ParentCostCenterId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class JournalConfiguration : BaseEntityConfiguration<Journal>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Journal> builder)
        {
            builder.ToTable("Journal");
            builder.Property(j => j.Code).IsRequired().HasMaxLength(50);
            builder.Property(j => j.Name).IsRequired().HasMaxLength(200);
            builder.HasIndex(j => new { j.TenantId, j.Code }).IsUnique().HasDatabaseName("IX_Journal_Code");
            builder.HasOne(j => j.DefaultDebitAccount).WithMany().HasForeignKey(j => j.DefaultDebitAccountId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(j => j.DefaultCreditAccount).WithMany().HasForeignKey(j => j.DefaultCreditAccountId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class JournalEntryConfiguration : BaseEntityConfiguration<JournalEntry>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<JournalEntry> builder)
        {
            builder.ToTable("JournalEntry");
            builder.Property(e => e.EntryNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.TotalDebit).HasPrecision(18, 2);
            builder.Property(e => e.TotalCredit).HasPrecision(18, 2);
            builder.HasIndex(e => new { e.TenantId, e.EntryNumber }).IsUnique().HasDatabaseName("IX_JournalEntry_Number");
            builder.HasOne(e => e.Journal).WithMany().HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class JournalEntryLineConfiguration : BaseEntityConfiguration<JournalEntryLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<JournalEntryLine> builder)
        {
            builder.ToTable("JournalEntryLine");
            builder.Property(l => l.Debit).HasPrecision(18, 2);
            builder.Property(l => l.Credit).HasPrecision(18, 2);
            builder.HasOne(l => l.JournalEntry).WithMany(e => e.Lines).HasForeignKey(l => l.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(l => l.CostCenter).WithMany().HasForeignKey(l => l.CostCenterId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class InvoiceConfiguration : BaseEntityConfiguration<Invoice>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoice");
            builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
            builder.Property(i => i.SubTotal).HasPrecision(18, 2);
            builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
            builder.Property(i => i.Total).HasPrecision(18, 2);
            builder.Property(i => i.PaidAmount).HasPrecision(18, 2);
            builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique().HasDatabaseName("IX_Invoice_Number");
            builder.HasOne(i => i.Customer).WithMany().HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(i => i.Vendor).WithMany().HasForeignKey(i => i.VendorId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(i => i.SalesOrder).WithMany().HasForeignKey(i => i.SalesOrderId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(i => i.PurchaseOrder).WithMany().HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(i => i.JournalEntry).WithMany().HasForeignKey(i => i.JournalEntryId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class InvoiceLineConfiguration : BaseEntityConfiguration<InvoiceLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<InvoiceLine> builder)
        {
            builder.ToTable("InvoiceLine");
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
            builder.Property(l => l.DiscountPercent).HasPrecision(5, 2);
            builder.Property(l => l.TaxPercent).HasPrecision(5, 2);
            builder.HasOne(l => l.Invoice).WithMany(i => i.Lines).HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class PaymentConfiguration : BaseEntityConfiguration<Payment>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payment");
            builder.Property(p => p.PaymentNumber).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.HasIndex(p => new { p.TenantId, p.PaymentNumber }).IsUnique().HasDatabaseName("IX_Payment_Number");
            builder.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.JournalEntry).WithMany().HasForeignKey(p => p.JournalEntryId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(p => p.BankAccount).WithMany().HasForeignKey(p => p.BankAccountId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class BudgetConfiguration : BaseEntityConfiguration<Budget>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budget");
            builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
            builder.Property(b => b.PlannedAmount).HasPrecision(18, 2);
            builder.Property(b => b.ActualAmount).HasPrecision(18, 2);
            builder.HasOne(b => b.Account).WithMany().HasForeignKey(b => b.AccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(b => b.CostCenter).WithMany().HasForeignKey(b => b.CostCenterId).OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class StockMoveConfiguration : BaseEntityConfiguration<StockMove>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockMove> builder)
        {
            builder.ToTable("StockMove");
            builder.Property(m => m.MoveNumber).IsRequired().HasMaxLength(50);
            builder.Property(m => m.Quantity).HasPrecision(18, 4);
            builder.HasIndex(m => new { m.TenantId, m.MoveNumber }).IsUnique().HasDatabaseName("IX_StockMove_Number");
            builder.HasOne(m => m.SalesOrder).WithMany().HasForeignKey(m => m.SalesOrderId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(m => m.PurchaseOrder).WithMany().HasForeignKey(m => m.PurchaseOrderId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class StockMoveLineConfiguration : BaseEntityConfiguration<StockMoveLine>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockMoveLine> builder)
        {
            builder.ToTable("StockMoveLine");
            builder.Property(l => l.Quantity).HasPrecision(18, 4);
            builder.HasOne(l => l.StockMove).WithMany(m => m.MoveLines).HasForeignKey(l => l.StockMoveId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
