using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InTagLogicLayer.Asset;
using InTagLogicLayer.Document;
using InTagLogicLayer.Manufacturing;
using InTagLogicLayer.Maintenance;
using InTagLogicLayer.Inventory;
using InTagLogicLayer.Workflow;
using InTagLogicLayer.Integration;

namespace InTagWeb.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController, Authorize]
    public class PlatformApiController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;
        private readonly IAssetService _assetService;
        private readonly IDocumentService _docService;
        private readonly IManufacturingService _mfgService;
        private readonly IMaintenanceService _mntService;
        private readonly IInventoryService _invService;
        private readonly IWorkflowService _wfService;

        public PlatformApiController(
            IIntegrationService integrationService,
            IAssetService assetService,
            IDocumentService docService,
            IManufacturingService mfgService,
            IMaintenanceService mntService,
            IInventoryService invService,
            IWorkflowService wfService)
        {
            _integrationService = integrationService;
            _assetService = assetService;
            _docService = docService;
            _mfgService = mfgService;
            _mntService = mntService;
            _invService = invService;
            _wfService = wfService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetExecutiveDashboard()
            => Ok(await _integrationService.GetExecutiveDashboardAsync());

        [HttpGet("compliance")]
        public async Task<IActionResult> GetComplianceReport()
            => Ok(await _integrationService.GetComplianceReportAsync());

        [HttpGet("audit-trail")]
        public async Task<IActionResult> GetAuditTrail([FromQuery] InTagViewModelLayer.Integration.AuditTrailFilterVm filter)
            => Ok(await _integrationService.GetAuditTrailAsync(filter));

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKPIs()
        {
            var dashboard = await _integrationService.GetExecutiveDashboardAsync();
            return Ok(new
            {
                assets = new { total = dashboard.TotalAssets, value = dashboard.TotalAssetValue, attention = dashboard.AssetsRequiringAttention },
                documents = new { total = dashboard.TotalDocuments, compliance = dashboard.DocumentCompliancePercent, overdue = dashboard.DocumentsOverdueReview },
                manufacturing = new { inProgress = dashboard.ProductionOrdersInProgress, qualityRate = dashboard.QualityPassRate },
                maintenance = new { openWOs = dashboard.OpenWorkOrders, overdue = dashboard.OverdueWorkOrders, pmCompliance = dashboard.PMCompliancePercent, slaCompliance = dashboard.SLACompliancePercent },
                inventory = new { value = dashboard.InventoryValue, outOfStock = dashboard.OutOfStockItems, expiring = dashboard.ExpiringItems },
                workflows = new { pending = dashboard.PendingApprovals, completed = dashboard.CompletedWorkflows, escalated = dashboard.EscalatedWorkflows }
            });
        }
    }
}
