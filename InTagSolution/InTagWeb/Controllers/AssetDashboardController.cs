using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagViewModelLayer.Asset;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    [RequireModule(PlatformModule.Asset)]
    public class AssetDashboardController : Controller
    {
        private readonly IAssetService _assetService;

        public AssetDashboardController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        // GET: /AssetDashboard
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Asset Dashboard";
            ViewData["Module"] = "Assets";
            var dashboard = await _assetService.GetDashboardAsync();
            return View(dashboard);
        }

        // GET: /AssetDashboard/ExportRegister
        public async Task<IActionResult> ExportRegister([FromQuery] AssetFilterVm filter)
        {
            var bytes = await _assetService.ExportAssetRegisterAsync(filter);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AssetRegister_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET: /AssetDashboard/ExportDepreciation?year=2026
        public async Task<IActionResult> ExportDepreciation(int? year)
        {
            var fiscalYear = year ?? DateTime.Now.Year;
            var bytes = await _assetService.ExportDepreciationScheduleAsync(fiscalYear);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DepreciationSchedule_FY{fiscalYear}.xlsx");
        }

        // GET: /AssetDashboard/ExportTCO
        public async Task<IActionResult> ExportTCO()
        {
            var bytes = await _assetService.ExportTCOReportAsync();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TCOAnalysis_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
