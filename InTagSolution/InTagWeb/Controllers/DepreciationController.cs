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
    public class DepreciationController : Controller
    {
        private readonly IAssetService _assetService;

        public DepreciationController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        // GET: /Depreciation — Summary dashboard
        public async Task<IActionResult> Index(int? year)
        {
            ViewData["Title"] = "Depreciation";
            ViewData["Module"] = "Assets";

            var summary = await _assetService.GetDepreciationSummaryAsync(year);
            return View(summary);
        }

        // GET: /Depreciation/History/5 — Single asset history
        public async Task<IActionResult> History(int id)
        {
            try
            {
                var asset = await _assetService.GetByIdAsync(id);
                var history = await _assetService.GetDepreciationHistoryAsync(id);

                ViewData["Title"] = $"Depreciation — {asset.AssetCode}";
                ViewData["Module"] = "Assets";
                ViewBag.Asset = asset;

                return View(history);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: /Depreciation/Forecast/5 — Projection
        public async Task<IActionResult> Forecast(int id, int months = 24)
        {
            try
            {
                var asset = await _assetService.GetByIdAsync(id);
                var history = await _assetService.GetDepreciationHistoryAsync(id);
                var forecast = await _assetService.ForecastDepreciationAsync(id, months);

                ViewData["Title"] = $"Forecast — {asset.AssetCode}";
                ViewData["Module"] = "Assets";
                ViewBag.Asset = asset;
                ViewBag.History = history;

                return View(forecast);
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Asset", new { id });
            }
        }

        // GET: /Depreciation/Run — Batch run form
        public IActionResult Run()
        {
            ViewData["Title"] = "Run Depreciation";
            ViewData["Module"] = "Assets";

            return View(new BulkDepreciationRunVm
            {
                FiscalYear = DateTimeOffset.UtcNow.Year,
                FiscalMonth = DateTimeOffset.UtcNow.Month
            });
        }

        // POST: /Depreciation/Run — Execute batch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(BulkDepreciationRunVm model)
        {
            ViewData["Title"] = "Depreciation Results";
            ViewData["Module"] = "Assets";

            var result = await _assetService.RunBulkDepreciationDetailedAsync(
                model.FiscalYear, model.FiscalMonth);

            return View("RunResult", result);
        }
    }
}
