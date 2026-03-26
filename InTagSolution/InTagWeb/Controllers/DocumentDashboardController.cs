using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Document;
using InTagViewModelLayer.Document;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InTagWeb.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [RequireModule(PlatformModule.Document)]
    public class DocumentDashboardController : Controller
    {
        private readonly IDocumentService _docService;

        public DocumentDashboardController(IDocumentService docService)
        {
            _docService = docService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Module"] = "Documents";
            var dashboard = await _docService.GetDashboardAsync();
            return View(dashboard);
        }

        public async Task<IActionResult> ExportRegister([FromQuery] DocumentFilterVm filter)
        {
            var bytes = await _docService.ExportDocumentRegisterAsync(filter);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DocumentRegister_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportCompliance()
        {
            var bytes = await _docService.ExportComplianceReportAsync();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ComplianceReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
