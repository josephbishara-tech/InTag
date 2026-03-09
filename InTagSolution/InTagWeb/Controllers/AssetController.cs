using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagViewModelLayer.Asset;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    [RequireModule(PlatformModule.Asset)]
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        // GET: /Asset
        public async Task<IActionResult> Index(AssetFilterVm filter)
        {
            ViewData["Title"] = "Assets";
            ViewData["Module"] = "Assets";

            var result = await _assetService.GetAllAsync(filter);

            await PopulateFilterDropdowns();
            ViewBag.CurrentFilter = filter;

            return View(result);
        }

        // GET: /Asset/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var asset = await _assetService.GetByIdAsync(id);
                ViewData["Title"] = asset.AssetCode;
                ViewData["Module"] = "Assets";
                return View(asset);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: /Asset/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Asset";
            ViewData["Module"] = "Assets";

            await PopulateFormDropdowns();
            return View(new AssetCreateVm
            {
                DepreciationMethod = DepreciationMethod.StraightLine,
                Category = AssetCategory.Equipment
            });
        }

        // POST: /Asset/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateFormDropdowns();
                return View(model);
            }

            try
            {
                var asset = await _assetService.CreateAsync(model);
                TempData["Success"] = $"Asset '{asset.AssetCode}' created successfully.";
                return RedirectToAction(nameof(Details), new { id = asset.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateFormDropdowns();
                return View(model);
            }
        }

        // GET: /Asset/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var asset = await _assetService.GetByIdAsync(id);
                ViewData["Title"] = $"Edit {asset.AssetCode}";
                ViewData["Module"] = "Assets";

                var model = new AssetUpdateVm
                {
                    Name = asset.Name,
                    Description = asset.Description,
                    Category = asset.Category,
                    AssetTypeId = 0, // Will be set from detail
                    SalvageValue = asset.SalvageValue,
                    SerialNumber = asset.SerialNumber,
                    Barcode = asset.Barcode,
                    Manufacturer = asset.Manufacturer,
                    ModelNumber = asset.ModelNumber,
                    WarrantyStartDate = asset.WarrantyStartDate,
                    WarrantyEndDate = asset.WarrantyEndDate,
                    Notes = asset.Notes
                };

                ViewBag.AssetId = id;
                ViewBag.AssetCode = asset.AssetCode;
                await PopulateFormDropdowns();
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Asset/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetUpdateVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AssetId = id;
                await PopulateFormDropdowns();
                return View(model);
            }

            try
            {
                await _assetService.UpdateAsync(id, model);
                TempData["Success"] = "Asset updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AssetId = id;
                await PopulateFormDropdowns();
                return View(model);
            }
        }

        // POST: /Asset/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _assetService.SoftDeleteAsync(id);
                TempData["Success"] = "Asset deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: /Asset/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, AssetStatus newStatus)
        {
            try
            {
                await _assetService.ChangeStatusAsync(id, newStatus);
                TempData["Success"] = $"Status changed to {newStatus}.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Asset/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(AssetTransferCreateVm model)
        {
            try
            {
                var result = await _assetService.TransferAsync(model);
                TempData["Success"] = $"Asset transferred to {result.ToLocationName}.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }

        // ── Helpers ──────────────────────────

        private async Task PopulateFilterDropdowns()
        {
            ViewBag.AssetTypes = new SelectList(await _assetService.GetAssetTypesAsync(), "Id", "Name");
            ViewBag.Locations = new SelectList(await _assetService.GetLocationsAsync(), "Id", "Name");
            ViewBag.Departments = new SelectList(await _assetService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Statuses = new SelectList(Enum.GetValues<AssetStatus>());
            ViewBag.Categories = new SelectList(Enum.GetValues<AssetCategory>());
        }

        private async Task PopulateFormDropdowns()
        {
            ViewBag.AssetTypes = new SelectList(await _assetService.GetAssetTypesAsync(), "Id", "Name");
            ViewBag.Locations = new SelectList(await _assetService.GetLocationsAsync(), "Id", "Name");
            ViewBag.Departments = new SelectList(await _assetService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Vendors = new SelectList(await _assetService.GetVendorsAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(Enum.GetValues<AssetCategory>());
            ViewBag.DepreciationMethods = new SelectList(Enum.GetValues<DepreciationMethod>());
        }
    }
}
