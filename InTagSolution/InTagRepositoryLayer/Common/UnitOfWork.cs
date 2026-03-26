using InTagDataLayer.Context;
using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Inventory;
using InTagEntitiesLayer.Maintenance;
using InTagEntitiesLayer.Manufacturing;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Asset;
using InTagRepositoryLayer.Document;
using InTagRepositoryLayer.Inventory;
using InTagRepositoryLayer.Maintenance;
using InTagRepositoryLayer.Manufacturing;
using InTagRepositoryLayer.Workflow;


namespace InTagRepositoryLayer.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InTagDbContext _context;

        public UnitOfWork(InTagDbContext context)
        {
            _context = context;
            Assets = new AssetRepository(context);
            AssetTypes = new AssetTypeRepository(context);
            Locations = new GenericRepository<InTagEntitiesLayer.Asset.Location>(context);
            Departments = new GenericRepository<InTagEntitiesLayer.Asset.Department>(context);
            Vendors = new GenericRepository<InTagEntitiesLayer.Asset.Vendor>(context);
            DepreciationRecords = new GenericRepository<InTagEntitiesLayer.Asset.DepreciationRecord>(context);
            AssetTransfers = new GenericRepository<InTagEntitiesLayer.Asset.AssetTransfer>(context);
            Inspections = new GenericRepository<InTagEntitiesLayer.Asset.Inspection>(context);
            Documents = new DocumentRepository(context);
            DocumentRevisions = new GenericRepository<DocumentRevision>(context);
            DocumentFiles = new GenericRepository<DocumentFile>(context);
            ApprovalMatrices = new GenericRepository<ApprovalMatrix>(context);
            DistributionRecordsRepo = new GenericRepository<DistributionRecord>(context);
            ProductionOrders = new ProductionOrderRepository(context);
            Products = new GenericRepository<Product>(context);
            // WorkCenters = new GenericRepository<WorkCenter>(context);
            WorkCenters = new WorkCenterRepository(context);
            BillOfMaterials = new GenericRepository<BillOfMaterial>(context);
            BOMLines = new GenericRepository<BOMLine>(context);
            Routings = new GenericRepository<Routing>(context);
            RoutingOperations = new GenericRepository<RoutingOperation>(context);
            ProductionLogs = new GenericRepository<ProductionLog>(context);
            LotBatches = new GenericRepository<LotBatch>(context);
            QualityChecks = new GenericRepository<QualityCheck>(context);
            WorkOrders = new WorkOrderRepository(context);
            WorkOrderLabors = new GenericRepository<WorkOrderLabor>(context);
            WorkOrderParts = new GenericRepository<WorkOrderPart>(context);
            PMSchedules = new GenericRepository<PMSchedule>(context);
            FailureLogs = new GenericRepository<FailureLog>(context);
            StockItems = new StockItemRepository(context);
            Warehouses = new GenericRepository<Warehouse>(context);
            StorageBins = new GenericRepository<StorageBin>(context);
            InventoryTransactions = new GenericRepository<InventoryTransaction>(context);
            CycleCounts = new GenericRepository<CycleCount>(context);
            CycleCountLines = new GenericRepository<CycleCountLine>(context);
            WorkflowInstances = new WorkflowRepository(context);
            WorkflowDefinitions = new GenericRepository<WorkflowDefinition>(context);
            WorkflowSteps = new GenericRepository<WorkflowStep>(context);
            WorkflowActions = new GenericRepository<WorkflowAction>(context);
            Notifications = new GenericRepository<Notification>(context);
            TrackingRequests = new GenericRepository<TrackingRequest>(context);
            TrackingLines = new GenericRepository<TrackingLine>(context);
            UserFolders = new GenericRepository<UserFolder>(context);
            UserFiles = new GenericRepository<UserFile>(context);
            FileShares = new GenericRepository<InTagEntitiesLayer.Document.FileShare>(context);

        }



        // Asset Module
        public IAssetRepository Assets { get; }
        public IAssetTypeRepository AssetTypes { get; }
        public IGenericRepository<InTagEntitiesLayer.Asset.Location> Locations { get; }
        public IGenericRepository<InTagEntitiesLayer.Asset.Department> Departments { get; }
        public IGenericRepository<InTagEntitiesLayer.Asset.Vendor> Vendors { get; }
        public IGenericRepository<InTagEntitiesLayer.Asset.DepreciationRecord> DepreciationRecords { get; }
        public IGenericRepository<InTagEntitiesLayer.Asset.AssetTransfer> AssetTransfers { get; }
        public IGenericRepository<InTagEntitiesLayer.Asset.Inspection> Inspections { get; }
        public IGenericRepository<TrackingRequest> TrackingRequests { get; }
        public IGenericRepository<TrackingLine> TrackingLines { get; }

        // Document Module
        public IDocumentRepository Documents { get; }
        public IGenericRepository<DocumentRevision> DocumentRevisions { get; }
        public IGenericRepository<DocumentFile> DocumentFiles { get; }
        public IGenericRepository<ApprovalMatrix> ApprovalMatrices { get; }
        public IGenericRepository<DistributionRecord> DistributionRecordsRepo { get; }
        public IGenericRepository<UserFolder> UserFolders { get; }
        public IGenericRepository<UserFile> UserFiles { get; }
        public IGenericRepository<InTagEntitiesLayer.Document.FileShare> FileShares { get; }

        // Manufacturing Module
        public IProductionOrderRepository ProductionOrders { get; }
        public IGenericRepository<Product> Products { get; }
       // public IGenericRepository<WorkCenter> WorkCenters { get; }
        public IWorkCenterRepository WorkCenters { get; }
        public IGenericRepository<BillOfMaterial> BillOfMaterials { get; }
        public IGenericRepository<BOMLine> BOMLines { get; }
        public IGenericRepository<Routing> Routings { get; }
        public IGenericRepository<RoutingOperation> RoutingOperations { get; }
        public IGenericRepository<ProductionLog> ProductionLogs { get; }
        public IGenericRepository<LotBatch> LotBatches { get; }
        public IGenericRepository<QualityCheck> QualityChecks { get; }



        // Maintenance (CMMS) Module
        public IWorkOrderRepository WorkOrders { get; }
        public IGenericRepository<WorkOrderLabor> WorkOrderLabors { get; }
        public IGenericRepository<WorkOrderPart> WorkOrderParts { get; }
        public IGenericRepository<PMSchedule> PMSchedules { get; }
        public IGenericRepository<FailureLog> FailureLogs { get; }

        // Inventory Module
        public IStockItemRepository StockItems { get; }
        public IGenericRepository<Warehouse> Warehouses { get; }
        public IGenericRepository<StorageBin> StorageBins { get; }
        public IGenericRepository<InventoryTransaction> InventoryTransactions { get; }
        public IGenericRepository<CycleCount> CycleCounts { get; }
        public IGenericRepository<CycleCountLine> CycleCountLines { get; }

        // Workflow Module
        public IWorkflowRepository WorkflowInstances { get; }
        public IGenericRepository<WorkflowDefinition> WorkflowDefinitions { get; }
        public IGenericRepository<WorkflowStep> WorkflowSteps { get; }
        public IGenericRepository<WorkflowAction> WorkflowActions { get; }
        public IGenericRepository<Notification> Notifications { get; }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}
