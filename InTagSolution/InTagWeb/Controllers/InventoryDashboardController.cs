using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Inventory;
using InTagViewModelLayer.Inventory;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InTagWeb.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Inventory)]
    public class InventoryDashboardController : Controller
    {
        private readonly IInventoryService _invService;

        public InventoryDashboardController(IInventoryService invService)
        {
            _invService = invService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Inventory Dashboard";
            ViewData["Module"] = "Inventory";
            return View(await _invService.GetDashboardAsync());
        }

        public async Task<IActionResult> ExportStock()
        {
            var bytes = await _invService.ExportStockReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StockReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportTransactions([FromQuery] TransactionFilterVm filter)
        {
            var bytes = await _invService.ExportTransactionReportAsync(filter);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TransactionReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportValuation()
        {
            var bytes = await _invService.ExportValuationReportAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ValuationReport_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
