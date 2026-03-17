using InTagViewModelLayer.Integration;

namespace InTagLogicLayer.Integration
{
    public interface IIntegrationService
    {
        Task<ExecutiveDashboardVm> GetExecutiveDashboardAsync();
        Task<ComplianceReportVm> GetComplianceReportAsync();
        Task<AuditTrailResultVm> GetAuditTrailAsync(AuditTrailFilterVm filter);
        Task<byte[]> ExportExecutiveReportAsync();
    }
}
