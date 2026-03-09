using InTagEntitiesLayer.Enums;
using InTagViewModelLayer.Inventory;

namespace InTagLogicLayer.Inventory
{
    public interface IInventoryService
    {
        // Warehouses
        Task<IReadOnlyList<WarehouseListVm>> GetWarehousesAsync();
        Task<WarehouseListVm> CreateWarehouseAsync(WarehouseCreateVm model);
        Task<IReadOnlyList<StorageBinListVm>> GetBinsAsync(int warehouseId);
        Task CreateBinAsync(StorageBinCreateVm model);

        // Stock
        Task<StockListResultVm> GetStockAsync(StockFilterVm filter);
        Task<StockItemDetailVm> GetStockItemAsync(int id);
        Task UpdateStockLevelsAsync(StockLevelUpdateVm model);

        // Transactions
        Task<TransactionListResultVm> GetTransactionsAsync(TransactionFilterVm filter);
        Task<TransactionListItemVm> RecordTransactionAsync(TransactionCreateVm model);

        // Cycle Counts
        Task<IReadOnlyList<CycleCountListItemVm>> GetCycleCountsAsync();
        Task<CycleCountDetailVm> GetCycleCountByIdAsync(int id);
        Task<CycleCountDetailVm> CreateCycleCountAsync(CycleCountCreateVm model);
        Task UpdateCycleCountLineAsync(CycleCountLineUpdateVm model);
        Task<CycleCountDetailVm> CompleteCycleCountAsync(int id);

        // Valuation & Reorder
        Task<InventoryValuationVm> GetValuationAsync();
        Task<ReorderReportVm> GetReorderReportAsync();
        // Advanced
        Task<ABCAnalysisVm> GetABCAnalysisAsync();
        Task<StockAgingVm> GetStockAgingAsync();
        Task<ExpiryReportVm> GetExpiryReportAsync(int daysThreshold = 30);
        Task<InventoryTurnoverVm> GetTurnoverAnalysisAsync(int months = 12);

        // Dashboard & Reports
        Task<InventoryDashboardVm> GetDashboardAsync();
        Task<byte[]> ExportStockReportAsync();
        Task<byte[]> ExportTransactionReportAsync(TransactionFilterVm filter);
        Task<byte[]> ExportValuationReportAsync();
    }
}
