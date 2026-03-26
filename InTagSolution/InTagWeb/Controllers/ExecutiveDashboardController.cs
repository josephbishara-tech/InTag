using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InTagLogicLayer.Integration;
using InTagViewModelLayer.Integration;

namespace InTagWeb.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    public class ExecutiveDashboardController : Controller
    {
        private readonly IIntegrationService _integrationService;

        public ExecutiveDashboardController(IIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Module"] = "Dashboard";
            return View(await _integrationService.GetExecutiveDashboardAsync());
        }

        public async Task<IActionResult> Compliance()
        {
            ViewData["Title"] = "Compliance Report";
            ViewData["Module"] = "Dashboard";
            return View(await _integrationService.GetComplianceReportAsync());
        }

        public async Task<IActionResult> AuditTrail(AuditTrailFilterVm filter)
        {
            ViewData["Title"] = "Audit Trail";
            ViewData["Module"] = "Dashboard";
            ViewBag.CurrentFilter = filter;
            return View(await _integrationService.GetAuditTrailAsync(filter));
        }

        public async Task<IActionResult> ExportExecutiveReport()
        {
            var bytes = await _integrationService.ExportExecutiveReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ExecutiveReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
