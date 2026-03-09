using InTagEntitiesLayer.Enums;
using InTagViewModelLayer.Maintenance;

namespace InTagLogicLayer.Maintenance
{
    public interface IMaintenanceService
    {
        // Work Orders
        Task<WorkOrderDetailVm> GetWorkOrderByIdAsync(int id);
        Task<WorkOrderListResultVm> GetWorkOrdersAsync(WorkOrderFilterVm filter);
        Task<WorkOrderDetailVm> CreateWorkOrderAsync(WorkOrderCreateVm model);
        Task<WorkOrderDetailVm> ChangeStatusAsync(int id, WorkOrderStatus newStatus);
        Task<WorkOrderDetailVm> CompleteWorkOrderAsync(WorkOrderCompleteVm model);
        Task<WorkOrderDetailVm> AddLaborAsync(LaborEntryCreateVm model);
        Task<WorkOrderDetailVm> AddPartAsync(PartEntryCreateVm model);
        Task RemoveLaborAsync(int laborId);
        Task RemovePartAsync(int partId);

        // PM Schedules
        Task<PMScheduleDetailVm> GetPMScheduleByIdAsync(int id);
        Task<IReadOnlyList<PMScheduleListItemVm>> GetPMSchedulesAsync();
        Task<PMScheduleDetailVm> CreatePMScheduleAsync(PMScheduleCreateVm model);
        Task<PMScheduleDetailVm> TogglePMScheduleAsync(int id);
        Task<PMGenerationResultVm> GeneratePMWorkOrdersAsync();

        // Reliability
        Task<MTBFMTTRResultVm> CalculateMTBFMTTRAsync(int assetId);
        Task<IReadOnlyList<MTBFMTTRResultVm>> GetReliabilityOverviewAsync();

        // Meter readings
        Task<MeterReadingResultVm> RecordMeterReadingAsync(MeterReadingVm model);
        Task<PMGenerationResultVm> GenerateConditionBasedPMsAsync();

        // Failure analysis
        Task<FailureAnalysisVm> GetFailureAnalysisAsync(DateTimeOffset? from = null, DateTimeOffset? to = null);

        // SLA & Backlog
        Task<SLAReportVm> GetSLAReportAsync();
        Task<IReadOnlyList<MaintenanceCostByAssetVm>> GetMaintenanceCostByAssetAsync();

        // Dashboard & Reports
        Task<MaintenanceDashboardVm> GetDashboardAsync();
        Task<byte[]> ExportWorkOrderReportAsync();
        Task<byte[]> ExportPMComplianceReportAsync();
        Task<byte[]> ExportMaintenanceCostReportAsync();
    }
}
