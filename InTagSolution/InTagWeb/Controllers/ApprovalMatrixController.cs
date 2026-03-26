using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagLogicLayer.Document;
using InTagViewModelLayer.Document;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
   // [AllowAnonymous]
    [RequireModule(PlatformModule.Document)]
    public class ApprovalMatrixController : Controller
    {
        private readonly IApprovalMatrixService _matrixService;
        private readonly IAssetService _assetService;

        public ApprovalMatrixController(
            IApprovalMatrixService matrixService,
            IAssetService assetService)
        {
            _matrixService = matrixService;
            _assetService = assetService;
        }

        // GET: /ApprovalMatrix
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Approval Matrix";
            ViewData["Module"] = "Documents";
            var items = await _matrixService.GetAllAsync();
            return View(items);
        }

        // GET: /ApprovalMatrix/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Approval Rule";
            ViewData["Module"] = "Documents";
            await PopulateDropdowns();
            return View(new ApprovalMatrixCreateVm { ApproverLevel = 1 });
        }

        // POST: /ApprovalMatrix/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApprovalMatrixCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            try
            {
                await _matrixService.CreateAsync(model);
                TempData["Success"] = "Approval rule created.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdowns();
                return View(model);
            }
        }

        // POST: /ApprovalMatrix/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _matrixService.DeleteAsync(id);
                TempData["Success"] = "Approval rule deleted.";
            }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.DocumentTypes = new SelectList(Enum.GetValues<DocumentType>());
            ViewBag.Departments = new SelectList(await _assetService.GetDepartmentsAsync(), "Id", "Name");
        }
    }
}
