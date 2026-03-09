using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Manufacturing;
using InTagViewModelLayer.Manufacturing;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Manufacturing)]
    public class ProductionController : Controller
    {
        private readonly IManufacturingService _mfgService;

        public ProductionController(IManufacturingService mfgService)
        {
            _mfgService = mfgService;
        }

        // ── Orders ───────────────────────────

        public async Task<IActionResult> Index(ProductionOrderFilterVm filter)
        {
            ViewData["Title"] = "Production Orders";
            ViewData["Module"] = "Manufacturing";
            var result = await _mfgService.GetOrdersAsync(filter);
            await PopulateOrderDropdowns();
            ViewBag.CurrentFilter = filter;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _mfgService.GetOrderByIdAsync(id);
                ViewData["Title"] = order.OrderNumber;
                ViewData["Module"] = "Manufacturing";
                await PopulateLogDropdowns(id);
                return View(order);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Production Order";
            ViewData["Module"] = "Manufacturing";
            await PopulateCreateDropdowns();
            return View(new ProductionOrderCreateVm { Priority = ProductionPriority.Normal });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionOrderCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateCreateDropdowns(); return View(model); }
            try
            {
                var order = await _mfgService.CreateOrderAsync(model);
                TempData["Success"] = $"Order '{order.OrderNumber}' created.";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateCreateDropdowns();
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, ProductionOrderStatus newStatus)
        {
            try
            {
                await _mfgService.ChangeOrderStatusAsync(id, newStatus);
                TempData["Success"] = $"Status changed to {newStatus}.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordProduction(ProductionLogCreateVm model)
        {
            try
            {
                await _mfgService.RecordProductionAsync(model);
                TempData["Success"] = "Production recorded.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.ProductionOrderId });
        }

        // ── Lots ─────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLot(LotBatchCreateVm model, int returnOrderId)
        {
            try
            {
                await _mfgService.CreateLotBatchAsync(model);
                TempData["Success"] = "Lot created.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = returnOrderId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeLotStatus(int id, LotBatchStatus newStatus, int returnOrderId)
        {
            try
            {
                await _mfgService.ChangeLotStatusAsync(id, newStatus);
                TempData["Success"] = $"Lot status changed to {newStatus}.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = returnOrderId });
        }

        // ── Quality ──────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordQualityCheck(QualityCheckCreateVm model, int returnOrderId)
        {
            try
            {
                await _mfgService.RecordQualityCheckAsync(model);
                TempData["Success"] = "Quality check recorded.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = returnOrderId });
        }

        // ── Helpers ──────────────────────────

        private async Task PopulateOrderDropdowns()
        {
            ViewBag.Statuses = new SelectList(Enum.GetValues<ProductionOrderStatus>());
            ViewBag.Priorities = new SelectList(Enum.GetValues<ProductionPriority>());
            ViewBag.Products = new SelectList(await _mfgService.GetProductsAsync(), "Id", "Name");
        }

        private async Task PopulateCreateDropdowns()
        {
            var products = await _mfgService.GetProductsAsync();
            var boms = await _mfgService.GetBOMsAsync();
            var routings = await _mfgService.GetRoutingsAsync();
            ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
            ViewBag.BOMs = new SelectList(boms.Where(b => b.Status == BOMStatus.Active), "Id", "BOMCode");
            ViewBag.Routings = new SelectList(routings.Where(r => r.IsActive), "Id", "RoutingCode");
            ViewBag.Priorities = new SelectList(Enum.GetValues<ProductionPriority>());
        }

        private async Task PopulateLogDropdowns(int orderId)
        {
            var order = await _mfgService.GetOrderByIdAsync(orderId);
            ViewBag.Operations = new SelectList(order.Operations, "Id", "OperationName");
            ViewBag.WorkCenters = new SelectList(await _mfgService.GetWorkCentersAsync(), "Id", "Name");
            ViewBag.QualityResults = new SelectList(Enum.GetValues<QualityCheckResult>());
            ViewBag.LotStatuses = new SelectList(Enum.GetValues<LotBatchStatus>());
        }

        // GET: /Production/Schedule/5
        public async Task<IActionResult> Schedule(int id)
        {
            try
            {
                var order = await _mfgService.GetOrderByIdAsync(id);
                ViewData["Title"] = $"Schedule — {order.OrderNumber}";
                ViewData["Module"] = "Manufacturing";
                ViewBag.Order = order;
                return View();
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // POST: /Production/Schedule/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(int id, DateTimeOffset startDate)
        {
            try
            {
                var result = await _mfgService.ScheduleOrderAsync(id, startDate);
                ViewData["Title"] = $"Schedule — {result.OrderNumber}";
                ViewData["Module"] = "Manufacturing";
                return View("ScheduleResult", result);
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: /Production/Cost/5
        public async Task<IActionResult> Cost(int id)
        {
            try
            {
                var result = await _mfgService.CalculateOrderCostAsync(id);
                ViewData["Title"] = $"Cost Analysis — {result.OrderNumber}";
                ViewData["Module"] = "Manufacturing";
                return View(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // GET: /Production/Capacity
        public async Task<IActionResult> Capacity(int? days)
        {
            ViewData["Title"] = "Work Center Capacity";
            ViewData["Module"] = "Manufacturing";
            var range = days ?? 14;
            var from = DateTimeOffset.UtcNow;
            var to = from.AddDays(range);
            ViewBag.Days = range;
            var result = await _mfgService.GetCapacityOverviewAsync(from, to);
            return View(result);
        }

    }
}
