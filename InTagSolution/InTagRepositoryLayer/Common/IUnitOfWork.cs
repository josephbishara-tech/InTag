using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Inventory;
using InTagEntitiesLayer.Maintenance;
using InTagEntitiesLayer.Manufacturing;
using InTagRepositoryLayer.Asset;
using InTagRepositoryLayer.Document;
using InTagRepositoryLayer.Inventory;
using InTagRepositoryLayer.Maintenance;
using InTagRepositoryLayer.Manufacturing;


namespace InTagRepositoryLayer.Common
{
    public interface IUnitOfWork : IDisposable
    {
        // Asset Module
        IAssetRepository Assets { get; }
        IAssetTypeRepository AssetTypes { get; }
        IGenericRepository<InTagEntitiesLayer.Asset.Location> Locations { get; }
        IGenericRepository<InTagEntitiesLayer.Asset.Department> Departments { get; }
        IGenericRepository<InTagEntitiesLayer.Asset.Vendor> Vendors { get; }
        IGenericRepository<InTagEntitiesLayer.Asset.DepreciationRecord> DepreciationRecords { get; }
        IGenericRepository<InTagEntitiesLayer.Asset.AssetTransfer> AssetTransfers { get; }
        IGenericRepository<InTagEntitiesLayer.Asset.Inspection> Inspections { get; }

        // Document Module
        IDocumentRepository Documents { get; }
        IGenericRepository<DocumentRevision> DocumentRevisions { get; }
        IGenericRepository<DocumentFile> DocumentFiles { get; }
        IGenericRepository<ApprovalMatrix> ApprovalMatrices { get; }
        IGenericRepository<DistributionRecord> DistributionRecordsRepo { get; }

        // Manufacturing Module
        IProductionOrderRepository ProductionOrders { get; }
        IGenericRepository<Product> Products { get; }
        IGenericRepository<WorkCenter> WorkCenters { get; }
        IGenericRepository<BillOfMaterial> BillOfMaterials { get; }
        IGenericRepository<BOMLine> BOMLines { get; }
        IGenericRepository<Routing> Routings { get; }
        IGenericRepository<RoutingOperation> RoutingOperations { get; }
        IGenericRepository<ProductionLog> ProductionLogs { get; }
        IGenericRepository<LotBatch> LotBatches { get; }
        IGenericRepository<QualityCheck> QualityChecks { get; }

        // Maintenance (CMMS) Module
        IWorkOrderRepository WorkOrders { get; }
        IGenericRepository<WorkOrderLabor> WorkOrderLabors { get; }
        IGenericRepository<WorkOrderPart> WorkOrderParts { get; }
        IGenericRepository<PMSchedule> PMSchedules { get; }
        IGenericRepository<FailureLog> FailureLogs { get; }
        // Inventory Module
        IStockItemRepository StockItems { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
        IGenericRepository<StorageBin> StorageBins { get; }
        IGenericRepository<InventoryTransaction> InventoryTransactions { get; }
        IGenericRepository<CycleCount> CycleCounts { get; }
        IGenericRepository<CycleCountLine> CycleCountLines { get; }
        Task<int> SaveChangesAsync();
    }
}
