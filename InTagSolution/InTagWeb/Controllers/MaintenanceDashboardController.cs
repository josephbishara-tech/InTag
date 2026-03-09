using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Maintenance;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Maintenance)]
    public class MaintenanceDashboardController : Controller
    {
        private readonly IMaintenanceService _mntService;

        public MaintenanceDashboardController(IMaintenanceService mntService)
        {
            _mntService = mntService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Maintenance Dashboard";
            ViewData["Module"] = "Maintenance";
            var dashboard = await _mntService.GetDashboardAsync();
            return View(dashboard);
        }

        public async Task<IActionResult> ExportWorkOrders()
        {
            var bytes = await _mntService.ExportWorkOrderReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"WorkOrderReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportPMCompliance()
        {
            var bytes = await _mntService.ExportPMComplianceReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PMCompliance_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportCosts()
        {
            var bytes = await _mntService.ExportMaintenanceCostReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"MaintenanceCosts_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
