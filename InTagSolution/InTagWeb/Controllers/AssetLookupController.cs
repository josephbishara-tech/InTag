using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagViewModelLayer.Asset;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InTagWeb.Controllers
{
    //[AllowAnonymous]
    [Authorize]
    [RequireModule(PlatformModule.Asset)]
    public class AssetLookupController : Controller
    {
        private readonly IAssetLookupService _lookupService;

        public AssetLookupController(IAssetLookupService lookupService)
        {
            _lookupService = lookupService;
        }

        // ══════════════════════════════════════
        //  ASSET TYPES
        // ══════════════════════════════════════

        public async Task<IActionResult> AssetTypes()
        {
            ViewData["Title"] = "Asset Types";
            ViewData["Module"] = "Assets";
            return View(await _lookupService.GetAssetTypesAsync());
        }

        public IActionResult CreateAssetType()
        {
            ViewData["Title"] = "New Asset Type";
            ViewData["Module"] = "Assets";
            return View(new AssetTypeCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssetType(AssetTypeCreateVm model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _lookupService.CreateAssetTypeAsync(model);
                TempData["Success"] = $"Asset type '{model.Name}' created.";
                return RedirectToAction(nameof(AssetTypes));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> EditAssetType(int id)
        {
            try
            {
                var model = await _lookupService.GetAssetTypeByIdAsync(id);
                ViewData["Title"] = $"Edit — {model.Name}";
                ViewData["Module"] = "Assets";
                return View(model);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssetType(AssetTypeUpdateVm model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _lookupService.UpdateAssetTypeAsync(model);
                TempData["Success"] = $"Asset type '{model.Name}' updated.";
                return RedirectToAction(nameof(AssetTypes));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // ══════════════════════════════════════
        //  LOCATIONS
        // ══════════════════════════════════════

        public async Task<IActionResult> Locations()
        {
            ViewData["Title"] = "Locations";
            ViewData["Module"] = "Assets";
            return View(await _lookupService.GetLocationsAsync());
        }

        public async Task<IActionResult> CreateLocation()
        {
            ViewData["Title"] = "New Location";
            ViewData["Module"] = "Assets";
            await PopulateLocationParents();
            return View(new LocationCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(LocationCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateLocationParents(); return View(model); }
            try
            {
                await _lookupService.CreateLocationAsync(model);
                TempData["Success"] = $"Location '{model.Name}' created.";
                return RedirectToAction(nameof(Locations));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateLocationParents();
                return View(model);
            }
        }

        public async Task<IActionResult> EditLocation(int id)
        {
            try
            {
                var model = await _lookupService.GetLocationByIdAsync(id);
                ViewData["Title"] = $"Edit — {model.Name}";
                ViewData["Module"] = "Assets";
                await PopulateLocationParents(id);
                return View(model);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(LocationUpdateVm model)
        {
            if (!ModelState.IsValid) { await PopulateLocationParents(model.Id); return View(model); }
            try
            {
                await _lookupService.UpdateLocationAsync(model);
                TempData["Success"] = $"Location '{model.Name}' updated.";
                return RedirectToAction(nameof(Locations));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateLocationParents(model.Id);
                return View(model);
            }
        }

        // ══════════════════════════════════════
        //  DEPARTMENTS
        // ══════════════════════════════════════

        public async Task<IActionResult> Departments()
        {
            ViewData["Title"] = "Departments";
            ViewData["Module"] = "Assets";
            return View(await _lookupService.GetDepartmentsAsync());
        }

        public async Task<IActionResult> CreateDepartment()
        {
            ViewData["Title"] = "New Department";
            ViewData["Module"] = "Assets";
            await PopulateDepartmentParents();
            return View(new DepartmentCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(DepartmentCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateDepartmentParents(); return View(model); }
            try
            {
                await _lookupService.CreateDepartmentAsync(model);
                TempData["Success"] = $"Department '{model.Name}' created.";
                return RedirectToAction(nameof(Departments));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDepartmentParents();
                return View(model);
            }
        }

        public async Task<IActionResult> EditDepartment(int id)
        {
            try
            {
                var model = await _lookupService.GetDepartmentByIdAsync(id);
                ViewData["Title"] = $"Edit — {model.Name}";
                ViewData["Module"] = "Assets";
                await PopulateDepartmentParents(id);
                return View(model);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(DepartmentUpdateVm model)
        {
            if (!ModelState.IsValid) { await PopulateDepartmentParents(model.Id); return View(model); }
            try
            {
                await _lookupService.UpdateDepartmentAsync(model);
                TempData["Success"] = $"Department '{model.Name}' updated.";
                return RedirectToAction(nameof(Departments));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDepartmentParents(model.Id);
                return View(model);
            }
        }

        // ══════════════════════════════════════
        //  VENDORS
        // ══════════════════════════════════════

        public async Task<IActionResult> Vendors()
        {
            ViewData["Title"] = "Vendors";
            ViewData["Module"] = "Assets";
            return View(await _lookupService.GetVendorsAsync());
        }

        public IActionResult CreateVendor()
        {
            ViewData["Title"] = "New Vendor";
            ViewData["Module"] = "Assets";
            return View(new VendorCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVendor(VendorCreateVm model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _lookupService.CreateVendorAsync(model);
                TempData["Success"] = $"Vendor '{model.Name}' created.";
                return RedirectToAction(nameof(Vendors));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> EditVendor(int id)
        {
            try
            {
                var model = await _lookupService.GetVendorByIdAsync(id);
                ViewData["Title"] = $"Edit — {model.Name}";
                ViewData["Module"] = "Assets";
                return View(model);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVendor(VendorUpdateVm model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _lookupService.UpdateVendorAsync(model);
                TempData["Success"] = $"Vendor '{model.Name}' updated.";
                return RedirectToAction(nameof(Vendors));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // ── Dropdown Helpers ─────────────────

        private async Task PopulateLocationParents(int? excludeId = null)
        {
            var locations = await _lookupService.GetLocationsAsync();
            var filtered = excludeId.HasValue ? locations.Where(l => l.Id != excludeId.Value) : locations;
            ViewBag.ParentLocations = new SelectList(filtered, "Id", "Name");
        }

        private async Task PopulateDepartmentParents(int? excludeId = null)
        {
            var departments = await _lookupService.GetDepartmentsAsync();
            var filtered = excludeId.HasValue ? departments.Where(d => d.Id != excludeId.Value) : departments;
            ViewBag.ParentDepartments = new SelectList(filtered, "Id", "Name");
        }
    }
}
