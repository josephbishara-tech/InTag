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
    public class WorkOrderController : Controller
    {
        private readonly IMaintenanceService _mntService;
        private readonly IAssetService _assetService;

        public WorkOrderController(IMaintenanceService mntService, IAssetService assetService)
        {
            _mntService = mntService;
            _assetService = assetService;
        }

        public async Task<IActionResult> Index(WorkOrderFilterVm filter)
        {
            ViewData["Title"] = "Work Orders";
            ViewData["Module"] = "Maintenance";
            var result = await _mntService.GetWorkOrdersAsync(filter);
            await PopulateFilterDropdowns();
            ViewBag.CurrentFilter = filter;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var wo = await _mntService.GetWorkOrderByIdAsync(id);
                ViewData["Title"] = wo.WorkOrderNumber;
                ViewData["Module"] = "Maintenance";
                return View(wo);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Work Order";
            ViewData["Module"] = "Maintenance";
            await PopulateCreateDropdowns();
            return View(new WorkOrderCreateVm { Priority = WorkOrderPriority.Medium, Type = WorkOrderType.Corrective });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrderCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateCreateDropdowns(); return View(model); }
            try
            {
                var wo = await _mntService.CreateWorkOrderAsync(model);
                TempData["Success"] = $"Work order '{wo.WorkOrderNumber}' created.";
                return RedirectToAction(nameof(Details), new { id = wo.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateCreateDropdowns();
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, WorkOrderStatus newStatus)
        {
            try
            {
                await _mntService.ChangeStatusAsync(id, newStatus);
                TempData["Success"] = $"Status changed to {newStatus}.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /WorkOrder/Complete/5
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var wo = await _mntService.GetWorkOrderByIdAsync(id);
                ViewData["Title"] = $"Complete — {wo.WorkOrderNumber}";
                ViewData["Module"] = "Maintenance";
                ViewBag.WorkOrder = wo;
                return View(new WorkOrderCompleteVm { WorkOrderId = id });
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(WorkOrderCompleteVm model)
        {
            try
            {
                await _mntService.CompleteWorkOrderAsync(model);
                TempData["Success"] = "Work order completed.";
                return RedirectToAction(nameof(Details), new { id = model.WorkOrderId });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = model.WorkOrderId });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLabor(LaborEntryCreateVm model)
        {
            try { await _mntService.AddLaborAsync(model); TempData["Success"] = "Labor recorded."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.WorkOrderId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(PartEntryCreateVm model)
        {
            try { await _mntService.AddPartAsync(model); TempData["Success"] = "Part added."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.WorkOrderId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLabor(int laborId, int workOrderId)
        {
            try { await _mntService.RemoveLaborAsync(laborId); TempData["Success"] = "Labor entry removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Details), new { id = workOrderId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePart(int partId, int workOrderId)
        {
            try { await _mntService.RemovePartAsync(partId); TempData["Success"] = "Part removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Details), new { id = workOrderId });
        }

        private async Task PopulateFilterDropdowns()
        {
            ViewBag.Statuses = new SelectList(Enum.GetValues<WorkOrderStatus>());
            ViewBag.Types = new SelectList(Enum.GetValues<WorkOrderType>());
            ViewBag.Priorities = new SelectList(Enum.GetValues<WorkOrderPriority>());
        }

        private async Task PopulateCreateDropdowns()
        {
            var assets = await _assetService.GetAllAsync(new InTagViewModelLayer.Asset.AssetFilterVm { PageSize = 1000 });
            ViewBag.Assets = new SelectList(assets.Items, "Id", "Name");
            ViewBag.Types = new SelectList(Enum.GetValues<WorkOrderType>());
            ViewBag.Priorities = new SelectList(Enum.GetValues<WorkOrderPriority>());
            ViewBag.FailureTypes = new SelectList(Enum.GetValues<FailureType>());
        }
    }
}
