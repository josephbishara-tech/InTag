using InTagViewModelLayer.ERP;

namespace InTagLogicLayer.ERP
{
    public interface IPurchaseService
    {
        // RFQs
        Task<IReadOnlyList<RfqListVm>> GetRfqsAsync();
        Task<RfqDetailVm> GetRfqByIdAsync(int id);
        Task<RfqDetailVm> CreateRfqAsync(RfqCreateVm model);
        Task AddRfqLineAsync(RfqLineAddVm model);
        Task RemoveRfqLineAsync(int lineId);
        Task<RfqDetailVm> SendRfqAsync(int id);
        Task RecordRfqResponseAsync(RfqLineResponseVm model);
        Task<PurchaseOrderDetailVm> ConvertRfqToPurchaseOrderAsync(int id);
        Task CancelRfqAsync(int id);

        // Purchase Orders
        Task<IReadOnlyList<PurchaseOrderListVm>> GetPurchaseOrdersAsync();
        Task<PurchaseOrderDetailVm> GetPurchaseOrderByIdAsync(int id);
        Task<PurchaseOrderDetailVm> CreatePurchaseOrderAsync(PurchaseOrderCreateVm model);
        Task AddPurchaseOrderLineAsync(PurchaseOrderLineAddVm model);
        Task RemovePurchaseOrderLineAsync(int lineId);
        Task<PurchaseOrderDetailVm> ConfirmPurchaseOrderAsync(int id);
        Task CancelPurchaseOrderAsync(int id);

        // Dashboard
        Task<PurchaseDashboardVm> GetDashboardAsync();
    }
}
