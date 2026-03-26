using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagViewModelLayer.Asset;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InTagWeb.Controllers
{
    [Authorize]
    [RequireModule(PlatformModule.Asset)]
    public class AssetTrackingController : Controller
    {
        private readonly IAssetTrackingService _trackingService;
        private readonly IAssetLookupService _lookupService;

        public AssetTrackingController(IAssetTrackingService trackingService, IAssetLookupService lookupService)
        {
            _trackingService = trackingService;
            _lookupService = lookupService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Asset Tracking";
            ViewData["Module"] = "Assets";
            return View(await _trackingService.GetRequestsAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var req = await _trackingService.GetRequestByIdAsync(id);
                ViewData["Title"] = req.RequestNumber;
                ViewData["Module"] = "Assets";
                return View(req);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Tracking Request";
            ViewData["Module"] = "Assets";
            ViewBag.Locations = new SelectList(await _lookupService.GetLocationsAsync(), "Id", "Name");
            return View(new TrackingRequestCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrackingRequestCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Locations = new SelectList(await _lookupService.GetLocationsAsync(), "Id", "Name");
                return View(model);
            }
            try
            {
                var req = await _trackingService.CreateRequestAsync(model);
                TempData["Success"] = $"Tracking request '{req.RequestNumber}' created with {req.TotalAssets} assets.";
                return RedirectToAction(nameof(Details), new { id = req.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Locations = new SelectList(await _lookupService.GetLocationsAsync(), "Id", "Name");
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(int id)
        {
            try { await _trackingService.OpenRequestAsync(id); TempData["Success"] = "Request opened for scanning."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try { await _trackingService.CompleteRequestAsync(id); TempData["Success"] = "Tracking completed."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try { await _trackingService.CancelRequestAsync(id); TempData["Success"] = "Request cancelled."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
