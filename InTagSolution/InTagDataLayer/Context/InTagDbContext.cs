using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.ERP;
using InTagEntitiesLayer.Identity;
using InTagEntitiesLayer.Interfaces;
using InTagEntitiesLayer.Inventory;
using InTagEntitiesLayer.Maintenance;
using InTagEntitiesLayer.Manufacturing;
using InTagEntitiesLayer.Workflow;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InTagDataLayer.Context
{
    public class InTagDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly ITenantService _tenantService;

        public InTagDbContext(
            DbContextOptions<InTagDbContext> options,
            ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Asset Module
        public DbSet<AssetItem> Assets => Set<AssetItem>();
        public DbSet<AssetType> AssetTypes => Set<AssetType>();
        public DbSet<DepreciationRecord> DepreciationRecords => Set<DepreciationRecord>();
        public DbSet<AssetTransfer> AssetTransfers => Set<AssetTransfer>();
        public DbSet<Inspection> Inspections => Set<Inspection>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<TrackingRequest> TrackingRequests => Set<TrackingRequest>();
        public DbSet<TrackingLine> TrackingLines => Set<TrackingLine>();

        // Document Module
        public DbSet<InTagEntitiesLayer.Document.Document> Documents => Set<InTagEntitiesLayer.Document.Document>();
        public DbSet<DocumentRevision> DocumentRevisions => Set<DocumentRevision>();
        public DbSet<DocumentFile> DocumentFiles => Set<DocumentFile>();
        public DbSet<ApprovalMatrix> ApprovalMatrices => Set<ApprovalMatrix>();
        public DbSet<DistributionRecord> DistributionRecords => Set<DistributionRecord>();
        public DbSet<UserFolder> UserFolders => Set<UserFolder>();
        public DbSet<UserFile> UserFiles => Set<UserFile>();
        public DbSet<InTagEntitiesLayer.Document.FileShare> FileShares => Set<InTagEntitiesLayer.Document.FileShare>();
        public DbSet<MetadataTemplate> MetadataTemplates => Set<MetadataTemplate>();
        public DbSet<MetadataFieldDefinition> MetadataFieldDefinitions => Set<MetadataFieldDefinition>();
        public DbSet<DocumentMetadata> DocumentMetadatas => Set<DocumentMetadata>();
        public DbSet<DocumentTag> DocumentTags => Set<DocumentTag>();
        public DbSet<DocumentLink> DocumentLinks => Set<DocumentLink>();

        // Manufacturing Module
        public DbSet<Product> Products => Set<Product>();
        public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
        public DbSet<BillOfMaterial> BillOfMaterials => Set<BillOfMaterial>();
        public DbSet<BOMLine> BOMLines => Set<BOMLine>();
        public DbSet<Routing> Routings => Set<Routing>();
        public DbSet<RoutingOperation> RoutingOperations => Set<RoutingOperation>();
        public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
        public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
        public DbSet<LotBatch> LotBatches => Set<LotBatch>();
        public DbSet<QualityCheck> QualityChecks => Set<QualityCheck>();

        // Maintenance (CMMS) Module
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<WorkOrderLabor> WorkOrderLabors => Set<WorkOrderLabor>();
        public DbSet<WorkOrderPart> WorkOrderParts => Set<WorkOrderPart>();
        public DbSet<PMSchedule> PMSchedules => Set<PMSchedule>();
        public DbSet<FailureLog> FailureLogs => Set<FailureLog>();

        // Inventory Module
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<StorageBin> StorageBins => Set<StorageBin>();
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<CycleCount> CycleCounts => Set<CycleCount>();
        public DbSet<CycleCountLine> CycleCountLines => Set<CycleCountLine>();

        // Workflow Module
        public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
        public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
        public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
        public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();
        public DbSet<Notification> Notifications => Set<Notification>();

        // ERP - Sales
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Pricelist> Pricelists => Set<Pricelist>();
        public DbSet<PricelistLine> PricelistLines => Set<PricelistLine>();
        public DbSet<SalesTeam> SalesTeams => Set<SalesTeam>();
        public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
        public DbSet<Quotation> Quotations => Set<Quotation>();
        public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

        // ERP - Purchase
        public DbSet<Rfq> Rfqs => Set<Rfq>();
        public DbSet<RfqLine> RfqLines => Set<RfqLine>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

        // ERP - Accounting
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<CostCenter> CostCenters => Set<CostCenter>();
        public DbSet<Journal> Journals => Set<Journal>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Budget> Budgets => Set<Budget>();

        // ERP - Stock Move
        public DbSet<StockMove> StockMoves => Set<StockMove>();
        public DbSet<StockMoveLine> StockMoveLines => Set<StockMoveLine>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InTagDbContext).Assembly);

            // Apply global query filters for multi-tenancy and soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(InTagDbContext)
                        .GetMethod(nameof(ApplyGlobalFilters),
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder, _tenantService });
                }
            }
         
        }
        

        private static void ApplyGlobalFilters<TEntity>(ModelBuilder modelBuilder,ITenantService tenantService)
            where TEntity : BaseEntity
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(
                    e => e.IsActive
                         && (tenantService.GetCurrentTenantId() == Guid.Empty
                             || e.TenantId == tenantService.GetCurrentTenantId()));
            }

        public override int SaveChanges()
        {
            ApplyAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var now = DateTimeOffset.UtcNow;

            Guid userId;
            Guid tenantId;

            try
            {
                userId = _tenantService.GetCurrentUserId();
                tenantId = _tenantService.GetCurrentTenantId();
            }
            catch
            {
                // System operation — no user/tenant context
                return;
            }

            // Skip audit stamping for system operations (no tenant)
            if (tenantId == Guid.Empty)
                return;

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.CreatedByUserId = userId;
                        entry.Entity.TenantId = tenantId;
                        entry.Entity.IsActive = true;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = now;
                        entry.Entity.ModifiedByUserId = userId;
                        entry.Property(e => e.CreatedDate).IsModified = false;
                        entry.Property(e => e.CreatedByUserId).IsModified = false;
                        entry.Property(e => e.TenantId).IsModified = false;
                        break;
                }
            }
        }
    }
}
