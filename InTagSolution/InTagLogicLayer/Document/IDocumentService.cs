using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public interface IDocumentService
    {
        // CRUD
        Task<DocumentDetailVm> GetByIdAsync(int id);
        Task<DocumentListResultVm> GetAllAsync(DocumentFilterVm filter);
        Task<DocumentDetailVm> CreateAsync(DocumentCreateVm model);
        Task<DocumentDetailVm> UpdateAsync(int id, DocumentUpdateVm model);
        Task SoftDeleteAsync(int id);

        // Check-in / Check-out
        Task<DocumentDetailVm> CheckOutAsync(int id);
        Task<DocumentDetailVm> CheckInAsync(int id, RevisionCreateVm revision);
        Task<DocumentDetailVm> CancelCheckOutAsync(int id);

        // Revisions & Approval
        Task<DocumentDetailVm> CreateRevisionAsync(RevisionCreateVm model);
        Task<DocumentDetailVm> ApproveRevisionAsync(RevisionApprovalVm model);

        // Lifecycle
        Task<DocumentDetailVm> PublishAsync(int id);
        Task<DocumentDetailVm> ObsoleteAsync(int id);
        Task<DocumentDetailVm> ArchiveAsync(int id);

        // Distribution
        Task DistributeAsync(DistributionCreateVm model);
        Task AcknowledgeDistributionAsync(int distributionId);

        // Review
        Task<IReadOnlyList<DocumentListItemVm>> GetDueForReviewAsync();

        // Dashboard & Reports
        Task<DocumentDashboardVm> GetDashboardAsync();
        Task<byte[]> ExportDocumentRegisterAsync(DocumentFilterVm filter);
        Task<byte[]> ExportComplianceReportAsync();
    }
}
