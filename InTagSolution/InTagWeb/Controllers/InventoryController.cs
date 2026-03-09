using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Inventory;
using InTagLogicLayer.Manufacturing;
using InTagViewModelLayer.Inventory;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Inventory)]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _invService;
        private readonly IManufacturingService _mfgService;

        public InventoryController(IInventoryService invService, IManufacturingService mfgService)
        {
            _invService = invService;
            _mfgService = mfgService;
        }

        // ── Stock ────────────────────────────

        public async Task<IActionResult> Index(StockFilterVm filter)
        {
            ViewData["Title"] = "Inventory";
            ViewData["Module"] = "Inventory";
            var result = await _invService.GetStockAsync(filter);
            await PopulateStockDropdowns();
            ViewBag.CurrentFilter = filter;
            return View(result);
        }

        public async Task<IActionResult> StockDetail(int id)
        {
            try
            {
                var item = await _invService.GetStockItemAsync(id);
                ViewData["Title"] = $"{item.ProductCode} @ {item.WarehouseCode}";
                ViewData["Module"] = "Inventory";
                return View(item);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStockLevels(StockLevelUpdateVm model)
        {
            try
            {
                await _invService.UpdateStockLevelsAsync(model);
                TempData["Success"] = "Stock levels updated.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(StockDetail), new { id = model.StockItemId });
        }

        // ── Transactions ─────────────────────

        public async Task<IActionResult> Transactions(TransactionFilterVm filter)
        {
            ViewData["Title"] = "Transactions";
            ViewData["Module"] = "Inventory";
            var result = await _invService.GetTransactionsAsync(filter);
            await PopulateTransactionDropdowns();
            ViewBag.CurrentFilter = filter;
            return View(result);
        }

        public async Task<IActionResult> RecordTransaction()
        {
            ViewData["Title"] = "Record Transaction";
            ViewData["Module"] = "Inventory";
            await PopulateTransactionFormDropdowns();
            return View(new TransactionCreateVm { Type = TransactionType.Receipt });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordTransaction(TransactionCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTransactionFormDropdowns();
                return View(model);
            }
            try
            {
                var txn = await _invService.RecordTransactionAsync(model);
                TempData["Success"] = $"Transaction '{txn.TransactionNumber}' recorded.";
                return RedirectToAction(nameof(Transactions));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateTransactionFormDropdowns();
                return View(model);
            }
        }

        // ── Warehouses ───────────────────────

        public async Task<IActionResult> Warehouses()
        {
            ViewData["Title"] = "Warehouses";
            ViewData["Module"] = "Inventory";
            return View(await _invService.GetWarehousesAsync());
        }

        public IActionResult CreateWarehouse()
        {
            ViewData["Title"] = "New Warehouse";
            ViewData["Module"] = "Inventory";
            return View(new WarehouseCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWarehouse(WarehouseCreateVm model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _invService.CreateWarehouseAsync(model);
                TempData["Success"] = $"Warehouse '{model.Code}' created.";
                return RedirectToAction(nameof(Warehouses));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBin(StorageBinCreateVm model)
        {
            try
            {
                await _invService.CreateBinAsync(model);
                TempData["Success"] = $"Bin '{model.BinCode}' created.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Warehouses));
        }

        // ── Cycle Counts ─────────────────────

        public async Task<IActionResult> CycleCounts()
        {
            ViewData["Title"] = "Cycle Counts";
            ViewData["Module"] = "Inventory";
            ViewBag.Warehouses = new SelectList(await _invService.GetWarehousesAsync(), "Id", "Name");
            return View(await _invService.GetCycleCountsAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCycleCount(CycleCountCreateVm model)
        {
            try
            {
                var cc = await _invService.CreateCycleCountAsync(model);
                TempData["Success"] = $"Cycle count '{cc.CountNumber}' created with {cc.TotalLines} lines.";
                return RedirectToAction(nameof(CycleCountDetail), new { id = cc.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(CycleCounts));
            }
        }

        public async Task<IActionResult> CycleCountDetail(int id)
        {
            try
            {
                var cc = await _invService.GetCycleCountByIdAsync(id);
                ViewData["Title"] = cc.CountNumber;
                ViewData["Module"] = "Inventory";
                return View(cc);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCountLine(CycleCountLineUpdateVm model, int cycleCountId)
        {
            try
            {
                await _invService.UpdateCycleCountLineAsync(model);
                TempData["Success"] = "Count updated.";
            }
            catch (KeyNotFoundException) { TempData["Error"] = "Line not found."; }
            return RedirectToAction(nameof(CycleCountDetail), new { id = cycleCountId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteCycleCount(int id)
        {
            try
            {
                await _invService.CompleteCycleCountAsync(id);
                TempData["Success"] = "Cycle count completed and stock adjusted.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(CycleCountDetail), new { id });
        }

        // ── Valuation & Reorder ──────────────

        public async Task<IActionResult> Valuation()
        {
            ViewData["Title"] = "Inventory Valuation";
            ViewData["Module"] = "Inventory";
            return View(await _invService.GetValuationAsync());
        }

        public async Task<IActionResult> Reorder()
        {
            ViewData["Title"] = "Reorder Report";
            ViewData["Module"] = "Inventory";
            return View(await _invService.GetReorderReportAsync());
        }

        // ── Helpers ──────────────────────────

        private async Task PopulateStockDropdowns()
        {
            ViewBag.Warehouses = new SelectList(await _invService.GetWarehousesAsync(), "Id", "Name");
            ViewBag.Products = new SelectList(await _mfgService.GetProductsAsync(), "Id", "Name");
        }

        private async Task PopulateTransactionDropdowns()
        {
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues<TransactionType>());
            ViewBag.Warehouses = new SelectList(await _invService.GetWarehousesAsync(), "Id", "Name");
        }

        private async Task PopulateTransactionFormDropdowns()
        {
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues<TransactionType>());
            ViewBag.Products = new SelectList(await _mfgService.GetProductsAsync(), "Id", "Name");
            ViewBag.Warehouses = new SelectList(await _invService.GetWarehousesAsync(), "Id", "Name");
        }

        public async Task<IActionResult> ABCAnalysis()
        {
            ViewData["Title"] = "ABC Analysis";
            ViewData["Module"] = "Inventory";
            return View(await _invService.GetABCAnalysisAsync());
        }

        public async Task<IActionResult> StockAging()
        {
            ViewData["Title"] = "Stock Aging";
            ViewData["Module"] = "Inventory";
            return View(await _invService.GetStockAgingAsync());
        }

        public async Task<IActionResult> ExpiryTracking(int days = 30)
        {
            ViewData["Title"] = "Expiry Tracking";
            ViewData["Module"] = "Inventory";
            ViewBag.Days = days;
            return View(await _invService.GetExpiryReportAsync(days));
        }

        public async Task<IActionResult> Turnover(int months = 12)
        {
            ViewData["Title"] = "Inventory Turnover";
            ViewData["Module"] = "Inventory";
            ViewBag.Months = months;
            return View(await _invService.GetTurnoverAnalysisAsync(months));
        }
    }
}
