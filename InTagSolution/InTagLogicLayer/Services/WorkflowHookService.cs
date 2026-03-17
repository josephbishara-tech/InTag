using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Interfaces;
using InTagLogicLayer.Workflow;

namespace InTagLogicLayer.Services
{
    public class WorkflowHookService : IWorkflowHook
    {
        private readonly ILogger<WorkflowHookService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WorkflowHookService(ILogger<WorkflowHookService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // ── Asset Events ─────────────────────

        public async Task OnAssetStatusChangedAsync(int assetId, string oldStatus, string newStatus)
        {
            _logger.LogInformation("[Hook] Asset {Id} status: {Old} → {New}", assetId, oldStatus, newStatus);
            if (newStatus is "Decommissioned" or "Disposed")
                await TryStartWorkflowAsync(WorkflowCategory.AssetDisposal, "Asset", assetId, $"Asset #{assetId}");
        }

        public async Task OnAssetTransferredAsync(int assetId, int fromLocationId, int toLocationId)
        {
            _logger.LogInformation("[Hook] Asset {Id} transferred: {From} → {To}", assetId, fromLocationId, toLocationId);
            await TryStartWorkflowAsync(WorkflowCategory.AssetTransfer, "Asset", assetId, $"Asset #{assetId}");
        }

        public Task OnInspectionCompletedAsync(int assetId, int inspectionId, string conditionScore)
        {
            _logger.LogInformation("[Hook] Inspection {InspId} for asset {AssetId}: {Score}", inspectionId, assetId, conditionScore);
            return Task.CompletedTask;
        }

        public Task OnDepreciationRunCompletedAsync(string period, int assetsProcessed, decimal totalAmount)
        {
            _logger.LogInformation("[Hook] Depreciation {Period}: {Count} assets, {Amount:C}", period, assetsProcessed, totalAmount);
            return Task.CompletedTask;
        }

        // ── Document Events ──────────────────

        public async Task OnDocumentPublishedAsync(int documentId, string docNumber)
        {
            _logger.LogInformation("[Hook] Document {Doc} published", docNumber);
            await SendNotificationAsync(Guid.Empty, $"Document Published: {docNumber}",
                $"Document {docNumber} has been published and is now effective.", $"/Document/Details/{documentId}");
        }

        public Task OnDocumentReviewDueAsync(int documentId, string docNumber, DateTimeOffset dueDate)
        {
            _logger.LogWarning("[Hook] Document {Doc} review due: {Date}", docNumber, dueDate);
            return Task.CompletedTask;
        }

        public Task OnRevisionApprovedAsync(int documentId, string docNumber, string revisionNumber)
        {
            _logger.LogInformation("[Hook] Revision {Rev} approved for {Doc}", revisionNumber, docNumber);
            return Task.CompletedTask;
        }

        public Task OnRevisionRejectedAsync(int documentId, string docNumber, string revisionNumber)
        {
            _logger.LogWarning("[Hook] Revision {Rev} rejected for {Doc}", revisionNumber, docNumber);
            return Task.CompletedTask;
        }

        public Task OnDocumentDistributedAsync(int documentId, string docNumber, string recipientName)
        {
            _logger.LogInformation("[Hook] Document {Doc} distributed to {Recipient}", docNumber, recipientName);
            return Task.CompletedTask;
        }

        // ── Manufacturing Events ─────────────

        public async Task OnProductionOrderStatusChangedAsync(int orderId, string orderNumber, string oldStatus, string newStatus)
        {
            _logger.LogInformation("[Hook] Order {Number}: {Old} → {New}", orderNumber, oldStatus, newStatus);
            if (newStatus == "Released")
                await TryStartWorkflowAsync(WorkflowCategory.ProductionRelease, "ProductionOrder", orderId, orderNumber);
        }

        public async Task OnQualityCheckFailedAsync(int orderId, string orderNumber, string checkName)
        {
            _logger.LogWarning("[Hook] QC FAILED — Order {Number}, Check: {Check}", orderNumber, checkName);
            await SendNotificationAsync(Guid.Empty, $"Quality Failure: {orderNumber}",
                $"Quality check '{checkName}' failed on production order {orderNumber}.", $"/Production/Details/{orderId}");
        }

        public Task OnLotQuarantinedAsync(int lotId, string lotNumber, string productName)
        {
            _logger.LogWarning("[Hook] Lot {Lot} ({Product}) quarantined", lotNumber, productName);
            return Task.CompletedTask;
        }

        public Task OnProductionOrderOverdueAsync(int orderId, string orderNumber, int daysOverdue)
        {
            _logger.LogWarning("[Hook] Order {Number} overdue by {Days} days", orderNumber, daysOverdue);
            return Task.CompletedTask;
        }

        // ── Maintenance Events ───────────────

        public Task OnWorkOrderStatusChangedAsync(int workOrderId, string workOrderNumber, string oldStatus, string newStatus)
        {
            _logger.LogInformation("[Hook] WO {Number}: {Old} → {New}", workOrderNumber, oldStatus, newStatus);
            return Task.CompletedTask;
        }

        public async Task OnWorkOrderSLABreachedAsync(int workOrderId, string workOrderNumber, decimal actualHours, decimal slaHours)
        {
            _logger.LogWarning("[Hook] SLA BREACHED — WO {Number}: {Actual}h > {SLA}h", workOrderNumber, actualHours, slaHours);
            await SendNotificationAsync(Guid.Empty, $"SLA Breached: {workOrderNumber}",
                $"Work order {workOrderNumber} exceeded SLA target ({actualHours:N1}h vs {slaHours:N1}h).",
                $"/WorkOrder/Details/{workOrderId}");
        }

        public Task OnPMWorkOrderGeneratedAsync(int workOrderId, string workOrderNumber, string scheduleName)
        {
            _logger.LogInformation("[Hook] PM WO {Number} from schedule {PM}", workOrderNumber, scheduleName);
            return Task.CompletedTask;
        }

        public Task OnWorkOrderOverdueAsync(int workOrderId, string workOrderNumber, int daysOverdue)
        {
            _logger.LogWarning("[Hook] WO {Number} overdue by {Days} days", workOrderNumber, daysOverdue);
            return Task.CompletedTask;
        }

        public Task OnPMOverdueAsync(int scheduleId, string scheduleName, string assetCode, int daysOverdue)
        {
            _logger.LogWarning("[Hook] PM '{Name}' for {Asset} overdue by {Days} days", scheduleName, assetCode, daysOverdue);
            return Task.CompletedTask;
        }

        public Task OnCriticalWorkOrderCreatedAsync(int workOrderId, string workOrderNumber, string assetCode)
        {
            _logger.LogWarning("[Hook] CRITICAL WO {Number} for asset {Asset}", workOrderNumber, assetCode);
            return Task.CompletedTask;
        }

        // ── Inventory Events ─────────────────

        public async Task OnStockBelowReorderAsync(int stockItemId, string productCode, string warehouseCode, decimal quantity, decimal reorderPoint)
        {
            _logger.LogWarning("[Hook] Stock low — {Product} @ {WH}: {Qty} (reorder at {RP})", productCode, warehouseCode, quantity, reorderPoint);
            await SendNotificationAsync(Guid.Empty, $"Low Stock: {productCode} @ {warehouseCode}",
                $"Stock is at {quantity} units, below reorder point of {reorderPoint}.", $"/Inventory");
        }

        public Task OnStockExpiredAsync(int stockItemId, string productCode, string lotNumber, decimal quantity)
        {
            _logger.LogWarning("[Hook] EXPIRED — {Product} Lot {Lot}: {Qty}", productCode, lotNumber, quantity);
            return Task.CompletedTask;
        }

        public Task OnCycleCountCompletedAsync(string countNumber, int variances, decimal totalVarianceQty)
        {
            _logger.LogInformation("[Hook] Cycle count {Number}: {V} variances", countNumber, variances);
            return Task.CompletedTask;
        }

        // ── Helpers ──────────────────────────

        private async Task TryStartWorkflowAsync(WorkflowCategory category, string entityType, int entityId, string? reference)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var wfService = scope.ServiceProvider.GetService<IWorkflowService>();
                if (wfService != null)
                    await wfService.StartWorkflowByCategoryAsync(category, entityType, entityId, reference);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No workflow triggered for {Category} on {EntityType}:{EntityId}", category, entityType, entityId);
            }
        }

        private async Task SendNotificationAsync(Guid userId, string title, string message, string actionUrl)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notifService = scope.ServiceProvider.GetService<INotificationService>();
                if (notifService != null)
                    await notifService.SendAsync(userId, title, message, actionUrl, NotificationChannel.Both);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to send notification: {Title}", title);
            }
        }
    }
}
