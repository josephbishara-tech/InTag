using InTagEntitiesLayer.Enums;
using InTagViewModelLayer.Manufacturing;

namespace InTagLogicLayer.Manufacturing
{
    public interface IManufacturingService
    {
        // Products
        Task<ProductDetailVm> GetProductByIdAsync(int id);
        Task<IReadOnlyList<ProductListItemVm>> GetProductsAsync();
        Task<ProductDetailVm> CreateProductAsync(ProductCreateVm model);
        Task<ProductDetailVm> UpdateProductAsync(int id, ProductCreateVm model);

        // BOMs
        Task<BOMDetailVm> GetBOMByIdAsync(int id);
        Task<IReadOnlyList<BOMListItemVm>> GetBOMsAsync();
        Task<BOMDetailVm> CreateBOMAsync(BOMCreateVm model);
        Task<BOMDetailVm> AddBOMLineAsync(BOMLineCreateVm model);
        Task RemoveBOMLineAsync(int lineId);
        Task<BOMDetailVm> ActivateBOMAsync(int id);

        // Routings
        Task<RoutingDetailVm> GetRoutingByIdAsync(int id);
        Task<IReadOnlyList<RoutingDetailVm>> GetRoutingsAsync();
        Task<RoutingDetailVm> CreateRoutingAsync(RoutingCreateVm model);
        Task<RoutingDetailVm> AddOperationAsync(RoutingOperationCreateVm model);
        Task RemoveOperationAsync(int operationId);

        // Work Centers
        // Task<IReadOnlyList<WorkCenterListVm>> GetWorkCentersAsync();
        Task<IReadOnlyList<WorkCenterListItemVm>> GetWorkCentersAsync();
        Task<WorkCenterDetailVm> GetWorkCenterByIdAsync(int id);
        Task<WorkCenterUpdateVm> GetWorkCenterForEditAsync(int id);
        Task<WorkCenterDetailVm> CreateWorkCenterAsync(WorkCenterCreateVm model);
        Task UpdateWorkCenterAsync(WorkCenterUpdateVm model);

        // Production Orders
        Task<ProductionOrderDetailVm> GetOrderByIdAsync(int id);
        Task<ProductionOrderListResultVm> GetOrdersAsync(ProductionOrderFilterVm filter);
        Task<ProductionOrderDetailVm> CreateOrderAsync(ProductionOrderCreateVm model);
        Task<ProductionOrderDetailVm> ChangeOrderStatusAsync(int id, ProductionOrderStatus newStatus);
        Task<ProductionOrderDetailVm> RecordProductionAsync(ProductionLogCreateVm model);

        // Lot/Batch
        Task<LotBatchListVm> CreateLotBatchAsync(LotBatchCreateVm model);
        Task ChangeLotStatusAsync(int id, LotBatchStatus newStatus);

        // Quality
        Task<QualityCheckVm> RecordQualityCheckAsync(QualityCheckCreateVm model);

        // OEE
        Task<OEEResultVm> CalculateOEEAsync(int productionOrderId);

        // Advanced
        Task<BOMExplosionResultVm> ExplodeBOMAsync(int bomId, decimal quantity);
        Task<ScheduleResultVm> ScheduleOrderAsync(int orderId, DateTimeOffset startDate);
        Task<IReadOnlyList<WorkCenterCapacityVm>> GetCapacityOverviewAsync(DateTimeOffset from, DateTimeOffset to);
        Task<ProductionCostResultVm> CalculateOrderCostAsync(int orderId);

        // Dashboard & Reports
        Task<ManufacturingDashboardVm> GetDashboardAsync();
        Task<byte[]> ExportProductionReportAsync();
        Task<byte[]> ExportQualityReportAsync();
    }
}
