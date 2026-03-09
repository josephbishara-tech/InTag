using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Document;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;

namespace InTagLogicLayer.Document
{
    public class ApprovalMatrixService : IApprovalMatrixService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ApprovalMatrixService> _logger;

        public ApprovalMatrixService(IUnitOfWork uow, ILogger<ApprovalMatrixService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ApprovalMatrixVm>> GetAllAsync()
        {
            var items = await _uow.ApprovalMatrices.Query()
                .Include(a => a.Department)
                .OrderBy(a => a.DocumentType)
                .ThenBy(a => a.DepartmentId)
                .ThenBy(a => a.ApproverLevel)
                .ToListAsync();

            return items.Select(MapToVm).ToList();
        }

        public async Task<IReadOnlyList<ApprovalMatrixVm>> GetForDocumentTypeAsync(
            DocumentType type, int? departmentId)
        {
            var items = await _uow.ApprovalMatrices.Query()
                .Include(a => a.Department)
                .Where(a => a.DocumentType == type
                            && (a.DepartmentId == departmentId || a.DepartmentId == null))
                .OrderBy(a => a.ApproverLevel)
                .ToListAsync();

            return items.Select(MapToVm).ToList();
        }

        public async Task<ApprovalMatrixVm> CreateAsync(ApprovalMatrixCreateVm model)
        {
            // Check uniqueness
            var exists = await _uow.ApprovalMatrices.ExistsAsync(
                a => a.DocumentType == model.DocumentType
                     && a.DepartmentId == model.DepartmentId
                     && a.ApproverLevel == model.ApproverLevel);

            if (exists)
                throw new InvalidOperationException(
                    $"Approval level {model.ApproverLevel} already exists for this document type and department.");

            var entry = new ApprovalMatrix
            {
                DocumentType = model.DocumentType,
                DepartmentId = model.DepartmentId,
                ApproverLevel = model.ApproverLevel,
                ApproverRole = model.ApproverRole,
                ApproverUserId = model.ApproverUserId,
                EscalationHours = model.EscalationHours,
                Description = model.Description
            };

            await _uow.ApprovalMatrices.AddAsync(entry);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Approval matrix entry created: {Type} Level {Level} — {Role}",
                model.DocumentType, model.ApproverLevel, model.ApproverRole);

            return MapToVm(entry);
        }

        public async Task<ApprovalMatrixVm> UpdateAsync(int id, ApprovalMatrixCreateVm model)
        {
            var entry = await _uow.ApprovalMatrices.GetByIdAsync(id);
            if (entry == null) throw new KeyNotFoundException("Approval matrix entry not found.");

            entry.DocumentType = model.DocumentType;
            entry.DepartmentId = model.DepartmentId;
            entry.ApproverLevel = model.ApproverLevel;
            entry.ApproverRole = model.ApproverRole;
            entry.ApproverUserId = model.ApproverUserId;
            entry.EscalationHours = model.EscalationHours;
            entry.Description = model.Description;

            _uow.ApprovalMatrices.Update(entry);
            await _uow.SaveChangesAsync();

            return MapToVm(entry);
        }

        public async Task DeleteAsync(int id)
        {
            var entry = await _uow.ApprovalMatrices.GetByIdAsync(id);
            if (entry == null) throw new KeyNotFoundException("Approval matrix entry not found.");

            _uow.ApprovalMatrices.SoftDelete(entry);
            await _uow.SaveChangesAsync();
        }

        private static ApprovalMatrixVm MapToVm(ApprovalMatrix a) => new()
        {
            Id = a.Id,
            DocumentType = a.DocumentType,
            DepartmentId = a.DepartmentId,
            DepartmentName = a.Department?.Name,
            ApproverLevel = a.ApproverLevel,
            ApproverRole = a.ApproverRole,
            ApproverUserId = a.ApproverUserId,
            EscalationHours = a.EscalationHours,
            Description = a.Description
        };
    }
}
