using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagLogicLayer.Maintenance;
using InTagViewModelLayer.Maintenance;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Maintenance)]
    public class PMScheduleController : Controller
    {
        private readonly IMaintenanceService _mntService;
        private readonly IAssetService _assetService;

        public PMScheduleController(IMaintenanceService mntService, IAssetService assetService)
        {
            _mntService = mntService;
            _assetService = assetService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "PM Schedules";
            ViewData["Module"] = "Maintenance";
            return View(await _mntService.GetPMSchedulesAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var pm = await _mntService.GetPMScheduleByIdAsync(id);
                ViewData["Title"] = pm.Name;
                ViewData["Module"] = "Maintenance";
                return View(pm);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New PM Schedule";
            ViewData["Module"] = "Maintenance";
            await PopulateDropdowns();
            return View(new PMScheduleCreateVm { Frequency = PMScheduleFrequency.Monthly, TriggerType = PMTriggerType.Calendar });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PMScheduleCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateDropdowns(); return View(model); }
            try
            {
                var pm = await _mntService.CreatePMScheduleAsync(model);
                TempData["Success"] = $"PM schedule '{pm.Name}' created.";
                return RedirectToAction(nameof(Details), new { id = pm.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdowns();
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                var pm = await _mntService.TogglePMScheduleAsync(id);
                TempData["Success"] = pm.IsEnabled ? "Schedule enabled." : "Schedule disabled.";
            }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateWorkOrders()
        {
            var result = await _mntService.GeneratePMWorkOrdersAsync();
            TempData["Success"] = $"{result.WorkOrdersGenerated} work order(s) generated from {result.SchedulesProcessed} schedule(s).";
            ViewData["Title"] = "PM Generation Results";
            ViewData["Module"] = "Maintenance";
            return View("GenerateResult", result);
        }

        // ── Reliability ──────────────────────

        public async Task<IActionResult> Reliability()
        {
            ViewData["Title"] = "Asset Reliability (MTBF/MTTR)";
            ViewData["Module"] = "Maintenance";
            var results = await _mntService.GetReliabilityOverviewAsync();
            return View(results);
        }

        private async Task PopulateDropdowns()
        {
            var assets = await _assetService.GetAllAsync(new InTagViewModelLayer.Asset.AssetFilterVm { PageSize = 1000 });
            ViewBag.Assets = new SelectList(assets.Items, "Id", "Name");
            ViewBag.TriggerTypes = new SelectList(Enum.GetValues<PMTriggerType>());
            ViewBag.Frequencies = new SelectList(Enum.GetValues<PMScheduleFrequency>());
            ViewBag.Priorities = new SelectList(Enum.GetValues<WorkOrderPriority>());
            ViewBag.ConditionRatings = new SelectList(Enum.GetValues<ConditionRating>());
        }

        // GET: /PMSchedule/MeterReading/5
        public async Task<IActionResult> MeterReading(int id)
        {
            try
            {
                var pm = await _mntService.GetPMScheduleByIdAsync(id);
                ViewData["Title"] = $"Meter Reading — {pm.Name}";
                ViewData["Module"] = "Maintenance";
                ViewBag.Schedule = pm;
                return View(new MeterReadingVm { PMScheduleId = id });
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // POST: /PMSchedule/MeterReading
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MeterReading(MeterReadingVm model)
        {
            try
            {
                var result = await _mntService.RecordMeterReadingAsync(model);
                if (result.ThresholdReached)
                    TempData["Success"] = $"Threshold reached! Work order {result.GeneratedWorkOrderNumber} generated.";
                else
                    TempData["Success"] = $"Meter reading recorded: {result.CurrentReading} {result.MeterType}.";
                return RedirectToAction(nameof(Details), new { id = model.PMScheduleId });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = model.PMScheduleId });
            }
        }

        // POST: /PMSchedule/GenerateConditionBased
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateConditionBased()
        {
            var result = await _mntService.GenerateConditionBasedPMsAsync();
            TempData["Success"] = $"{result.WorkOrdersGenerated} condition-based work order(s) generated.";
            return View("GenerateResult", result);
        }

        // GET: /PMSchedule/FailureAnalysis
        public async Task<IActionResult> FailureAnalysis()
        {
            ViewData["Title"] = "Failure Analysis";
            ViewData["Module"] = "Maintenance";
            var result = await _mntService.GetFailureAnalysisAsync();
            return View(result);
        }

        // GET: /PMSchedule/SLAReport
        public async Task<IActionResult> SLAReport()
        {
            ViewData["Title"] = "SLA & Backlog Report";
            ViewData["Module"] = "Maintenance";
            var result = await _mntService.GetSLAReportAsync();
            return View(result);
        }

        // GET: /PMSchedule/CostByAsset
        public async Task<IActionResult> CostByAsset()
        {
            ViewData["Title"] = "Maintenance Cost by Asset";
            ViewData["Module"] = "Maintenance";
            var result = await _mntService.GetMaintenanceCostByAssetAsync();
            return View(result);
        }
    }
}
