namespace InTagEntitiesLayer.Interfaces
{
    public interface IWorkflowHook
    {
        // Asset events
        Task OnAssetStatusChangedAsync(int assetId, string oldStatus, string newStatus);
        Task OnAssetTransferredAsync(int assetId, int fromLocationId, int toLocationId);
        Task OnInspectionCompletedAsync(int assetId, int inspectionId, string conditionScore);
        Task OnDepreciationRunCompletedAsync(string period, int assetsProcessed, decimal totalAmount);

        // Document events
        Task OnDocumentPublishedAsync(int documentId, string docNumber);
        Task OnDocumentReviewDueAsync(int documentId, string docNumber, DateTimeOffset dueDate);
        Task OnRevisionApprovedAsync(int documentId, string docNumber, string revisionNumber);
        Task OnRevisionRejectedAsync(int documentId, string docNumber, string revisionNumber);
        Task OnDocumentDistributedAsync(int documentId, string docNumber, string recipientName);

        // Manufacturing events
        Task OnProductionOrderStatusChangedAsync(int orderId, string orderNumber, string oldStatus, string newStatus);
        Task OnQualityCheckFailedAsync(int orderId, string orderNumber, string checkName);
        Task OnLotQuarantinedAsync(int lotId, string lotNumber, string productName);
        Task OnProductionOrderOverdueAsync(int orderId, string orderNumber, int daysOverdue);

        // Maintenance events
        Task OnWorkOrderStatusChangedAsync(int workOrderId, string workOrderNumber, string oldStatus, string newStatus);
        Task OnWorkOrderSLABreachedAsync(int workOrderId, string workOrderNumber, decimal actualHours, decimal slaHours);
        Task OnPMWorkOrderGeneratedAsync(int workOrderId, string workOrderNumber, string scheduleName);

        // Maintenance — advanced events
        Task OnWorkOrderOverdueAsync(int workOrderId, string workOrderNumber, int daysOverdue);
        Task OnPMOverdueAsync(int scheduleId, string scheduleName, string assetCode, int daysOverdue);
        Task OnCriticalWorkOrderCreatedAsync(int workOrderId, string workOrderNumber, string assetCode);

        // Inventory events
        Task OnStockBelowReorderAsync(int stockItemId, string productCode, string warehouseCode, decimal quantity, decimal reorderPoint);
        Task OnStockExpiredAsync(int stockItemId, string productCode, string lotNumber, decimal quantity);
        Task OnCycleCountCompletedAsync(string countNumber, int variances, decimal totalVarianceQty);
    }
}
