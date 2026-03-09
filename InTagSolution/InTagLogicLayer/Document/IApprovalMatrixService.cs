using InTagEntitiesLayer.Document;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public interface IApprovalMatrixService
    {
        Task<IReadOnlyList<ApprovalMatrixVm>> GetAllAsync();
        Task<IReadOnlyList<ApprovalMatrixVm>> GetForDocumentTypeAsync(InTagEntitiesLayer.Enums.DocumentType type, int? departmentId);
        Task<ApprovalMatrixVm> CreateAsync(ApprovalMatrixCreateVm model);
        Task<ApprovalMatrixVm> UpdateAsync(int id, ApprovalMatrixCreateVm model);
        Task DeleteAsync(int id);
    }
}
