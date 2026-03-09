using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Manufacturing;
using InTagViewModelLayer.Manufacturing;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Manufacturing)]
    public class ManufacturingDashboardController : Controller
    {
        private readonly IManufacturingService _mfgService;

        public ManufacturingDashboardController(IManufacturingService mfgService)
        {
            _mfgService = mfgService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Manufacturing Dashboard";
            ViewData["Module"] = "Manufacturing";
            var dashboard = await _mfgService.GetDashboardAsync();
            return View(dashboard);
        }

        public async Task<IActionResult> ExportProduction()
        {
            var bytes = await _mfgService.ExportProductionReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ProductionReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportQuality()
        {
            var bytes = await _mfgService.ExportQualityReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"QualityReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
