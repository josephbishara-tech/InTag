using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Asset;
using InTagEntitiesLayer.Enums;
using InTagRepositoryLayer.Common;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Asset)]
    public class AssetLookupController : Controller
    {
        private readonly IUnitOfWork _uow;

        public AssetLookupController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ══════════════════════════════════════
        //  ASSET TYPES
        // ══════════════════════════════════════

        public async Task<IActionResult> AssetTypes()
        {
            ViewData["Title"] = "Asset Types";
            ViewData["Module"] = "Asset";
            var items = await _uow.AssetTypes.Query().OrderBy(t => t.Name).ToListAsync();
            return View(items);
        }

        public IActionResult CreateAssetType()
        {
            ViewData["Title"] = "New Asset Type";
            ViewData["Module"] = "Asset";
            return View(new AssetType());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssetType(AssetType model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _uow.AssetTypes.ExistsAsync(t => t.Name == model.Name))
            {
                ModelState.AddModelError("Name", "An asset type with this name already exists.");
                return View(model);
            }
            await _uow.AssetTypes.AddAsync(model);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Asset type '{model.Name}' created.";
            return RedirectToAction(nameof(AssetTypes));
        }

        public async Task<IActionResult> EditAssetType(int id)
        {
            var item = await _uow.AssetTypes.GetByIdAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = $"Edit — {item.Name}";
            ViewData["Module"] = "Asset";
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssetType(int id, AssetType model)
        {
            if (!ModelState.IsValid) return View(model);
            var item = await _uow.AssetTypes.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Name = model.Name;
            item.Description = model.Description;
            item.DefaultDepreciationMethod = model.DefaultDepreciationMethod;
            item.UsefulLifeMonths = model.UsefulLifeMonths;
            item.Category = model.Category;
            item.DefaultSalvageValuePercent = model.DefaultSalvageValuePercent;

            _uow.AssetTypes.Update(item);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Asset type '{item.Name}' updated.";
            return RedirectToAction(nameof(AssetTypes));
        }

        // ══════════════════════════════════════
        //  LOCATIONS
        // ══════════════════════════════════════

        public async Task<IActionResult> Locations()
        {
            ViewData["Title"] = "Locations";
            ViewData["Module"] = "Asset";
            var items = await _uow.Locations.Query()
                .Include(l => l.ParentLocation)
                .OrderBy(l => l.Name).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> CreateLocation()
        {
            ViewData["Title"] = "New Location";
            ViewData["Module"] = "Asset";
            await PopulateLocationParents();
            return View(new Location());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(Location model)
        {
            if (!ModelState.IsValid) { await PopulateLocationParents(); return View(model); }
            if (await _uow.Locations.ExistsAsync(l => l.Code == model.Code))
            {
                ModelState.AddModelError("Code", "A location with this code already exists.");
                await PopulateLocationParents();
                return View(model);
            }
            await _uow.Locations.AddAsync(model);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Location '{model.Name}' created.";
            return RedirectToAction(nameof(Locations));
        }

        public async Task<IActionResult> EditLocation(int id)
        {
            var item = await _uow.Locations.GetByIdAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = $"Edit — {item.Name}";
            ViewData["Module"] = "Asset";
            await PopulateLocationParents(id);
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(int id, Location model)
        {
            if (!ModelState.IsValid) { await PopulateLocationParents(id); return View(model); }
            var item = await _uow.Locations.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Code = model.Code;
            item.Name = model.Name;
            item.Address = model.Address;
            item.Building = model.Building;
            item.Floor = model.Floor;
            item.Room = model.Room;
            item.ParentLocationId = model.ParentLocationId;

            _uow.Locations.Update(item);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Location '{item.Name}' updated.";
            return RedirectToAction(nameof(Locations));
        }

        // ══════════════════════════════════════
        //  DEPARTMENTS
        // ══════════════════════════════════════

        public async Task<IActionResult> Departments()
        {
            ViewData["Title"] = "Departments";
            ViewData["Module"] = "Asset";
            var items = await _uow.Departments.Query()
                .Include(d => d.ParentDepartment)
                .OrderBy(d => d.Name).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> CreateDepartment()
        {
            ViewData["Title"] = "New Department";
            ViewData["Module"] = "Asset";
            await PopulateDepartmentParents();
            return View(new Department());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department model)
        {
            if (!ModelState.IsValid) { await PopulateDepartmentParents(); return View(model); }
            if (await _uow.Departments.ExistsAsync(d => d.Code == model.Code))
            {
                ModelState.AddModelError("Code", "A department with this code already exists.");
                await PopulateDepartmentParents();
                return View(model);
            }
            await _uow.Departments.AddAsync(model);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Department '{model.Name}' created.";
            return RedirectToAction(nameof(Departments));
        }

        public async Task<IActionResult> EditDepartment(int id)
        {
            var item = await _uow.Departments.GetByIdAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = $"Edit — {item.Name}";
            ViewData["Module"] = "Asset";
            await PopulateDepartmentParents(id);
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, Department model)
        {
            if (!ModelState.IsValid) { await PopulateDepartmentParents(id); return View(model); }
            var item = await _uow.Departments.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Code = model.Code;
            item.Name = model.Name;
            item.Description = model.Description;
            item.ParentDepartmentId = model.ParentDepartmentId;

            _uow.Departments.Update(item);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Department '{item.Name}' updated.";
            return RedirectToAction(nameof(Departments));
        }

        // ══════════════════════════════════════
        //  VENDORS
        // ══════════════════════════════════════

        public async Task<IActionResult> Vendors()
        {
            ViewData["Title"] = "Vendors";
            ViewData["Module"] = "Asset";
            var items = await _uow.Vendors.Query().OrderBy(v => v.Name).ToListAsync();
            return View(items);
        }

        public IActionResult CreateVendor()
        {
            ViewData["Title"] = "New Vendor";
            ViewData["Module"] = "Asset";
            return View(new Vendor());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVendor(Vendor model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _uow.Vendors.ExistsAsync(v => v.Code == model.Code))
            {
                ModelState.AddModelError("Code", "A vendor with this code already exists.");
                return View(model);
            }
            await _uow.Vendors.AddAsync(model);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Vendor '{model.Name}' created.";
            return RedirectToAction(nameof(Vendors));
        }

        public async Task<IActionResult> EditVendor(int id)
        {
            var item = await _uow.Vendors.GetByIdAsync(id);
            if (item == null) return NotFound();
            ViewData["Title"] = $"Edit — {item.Name}";
            ViewData["Module"] = "Asset";
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVendor(int id, Vendor model)
        {
            if (!ModelState.IsValid) return View(model);
            var item = await _uow.Vendors.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Code = model.Code;
            item.Name = model.Name;
            item.ContactPerson = model.ContactPerson;
            item.Email = model.Email;
            item.Phone = model.Phone;
            item.Address = model.Address;
            item.Website = model.Website;
            item.Notes = model.Notes;

            _uow.Vendors.Update(item);
            await _uow.SaveChangesAsync();
            TempData["Success"] = $"Vendor '{item.Name}' updated.";
            return RedirectToAction(nameof(Vendors));
        }

        // ── Helpers ──────────────────────────

        private async Task PopulateLocationParents(int? excludeId = null)
        {
            var locs = await _uow.Locations.Query().OrderBy(l => l.Name).ToListAsync();
            if (excludeId.HasValue) locs = locs.Where(l => l.Id != excludeId.Value).ToList();
            ViewBag.ParentLocations = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(locs, "Id", "Name");
        }

        private async Task PopulateDepartmentParents(int? excludeId = null)
        {
            var depts = await _uow.Departments.Query().OrderBy(d => d.Name).ToListAsync();
            if (excludeId.HasValue) depts = depts.Where(d => d.Id != excludeId.Value).ToList();
            ViewBag.ParentDepartments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(depts, "Id", "Name");
        }
    }
}
