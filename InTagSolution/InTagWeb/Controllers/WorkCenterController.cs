using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagLogicLayer.Manufacturing;
using InTagViewModelLayer.Manufacturing;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
    [RequireModule(PlatformModule.Manufacturing)]
    public class WorkCenterController : Controller
    {
        private readonly IManufacturingService _mfgService;
        private readonly IAssetLookupService _lookupService;

        public WorkCenterController(IManufacturingService mfgService, IAssetLookupService lookupService)
        {
            _mfgService = mfgService;
            _lookupService = lookupService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Work Centers";
            ViewData["Module"] = "Manufacturing";
            return View(await _mfgService.GetWorkCentersAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var wc = await _mfgService.GetWorkCenterByIdAsync(id);
                ViewData["Title"] = $"{wc.Code} — {wc.Name}";
                ViewData["Module"] = "Manufacturing";
                return View(wc);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Work Center";
            ViewData["Module"] = "Manufacturing";
            await PopulateDropdowns();
            return View(new WorkCenterCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkCenterCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateDropdowns(); return View(model); }
            try
            {
                var wc = await _mfgService.CreateWorkCenterAsync(model);
                TempData["Success"] = $"Work center '{wc.Code}' created.";
                return RedirectToAction(nameof(Details), new { id = wc.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdowns();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _mfgService.GetWorkCenterForEditAsync(id);
                ViewData["Title"] = $"Edit — {model.Code}";
                ViewData["Module"] = "Manufacturing";
                await PopulateDropdowns();
                return View(model);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkCenterUpdateVm model)
        {
            if (!ModelState.IsValid) { await PopulateDropdowns(); return View(model); }
            try
            {
                await _mfgService.UpdateWorkCenterAsync(model);
                TempData["Success"] = $"Work center '{model.Code}' updated.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdowns();
                return View(model);
            }
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.Statuses = new SelectList(Enum.GetValues<WorkCenterStatus>());
            ViewBag.Locations = new SelectList(await _lookupService.GetLocationsAsync(), "Id", "Name");
            ViewBag.Departments = new SelectList(await _lookupService.GetDepartmentsAsync(), "Id", "Name");
        }
    }
}
