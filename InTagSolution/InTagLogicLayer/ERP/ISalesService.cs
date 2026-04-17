using InTagViewModelLayer.ERP;

namespace InTagLogicLayer.ERP
{
    public interface ISalesService
    {
        // Customers
        Task<IReadOnlyList<CustomerListVm>> GetCustomersAsync();
        Task<CustomerDetailVm> GetCustomerByIdAsync(int id);
        Task<CustomerDetailVm> CreateCustomerAsync(CustomerCreateVm model);
        Task UpdateCustomerAsync(CustomerUpdateVm model);

        // Quotations
        Task<IReadOnlyList<QuotationListVm>> GetQuotationsAsync();
        Task<QuotationDetailVm> GetQuotationByIdAsync(int id);
        Task<QuotationDetailVm> CreateQuotationAsync(QuotationCreateVm model);
        Task AddQuotationLineAsync(QuotationLineAddVm model);
        Task RemoveQuotationLineAsync(int lineId);
        Task<QuotationDetailVm> SendQuotationAsync(int id);
        Task<SalesOrderDetailVm> ConfirmQuotationAsync(int id);
        Task CancelQuotationAsync(int id);

        // Sales Orders
        Task<IReadOnlyList<SalesOrderListVm>> GetSalesOrdersAsync();
        Task<SalesOrderDetailVm> GetSalesOrderByIdAsync(int id);
        Task<SalesOrderDetailVm> CreateSalesOrderAsync(SalesOrderCreateVm model);
        Task AddSalesOrderLineAsync(SalesOrderLineAddVm model);
        Task RemoveSalesOrderLineAsync(int lineId);
        Task<SalesOrderDetailVm> ConfirmSalesOrderAsync(int id);
        Task CancelSalesOrderAsync(int id);

        // Dashboard
        Task<SalesDashboardVm> GetDashboardAsync();

        // Pricelists
        Task<IReadOnlyList<PricelistListVm>> GetPricelistsAsync();
        Task<decimal?> GetProductPriceAsync(int productId, int? customerId, decimal quantity);

        // Sales Teams
        Task<IReadOnlyList<SalesTeamListVm>> GetSalesTeamsAsync();
    }
}
