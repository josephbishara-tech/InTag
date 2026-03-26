using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Manufacturing;
using InTagViewModelLayer.Manufacturing;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [RequireModule(PlatformModule.Manufacturing)]
    public class ProductCatalogController : Controller
    {
        private readonly IManufacturingService _mfgService;

        public ProductCatalogController(IManufacturingService mfgService)
        {
            _mfgService = mfgService;
        }

        // ── Products ─────────────────────────

        public async Task<IActionResult> Products()
        {
            ViewData["Title"] = "Products";
            ViewData["Module"] = "Manufacturing";
            return View(await _mfgService.GetProductsAsync());
        }

        public IActionResult CreateProduct()
        {
            ViewData["Title"] = "New Product";
            ViewData["Module"] = "Manufacturing";
            ViewBag.UOMs = new SelectList(Enum.GetValues<UnitOfMeasure>());
            return View(new ProductCreateVm { UOM = UnitOfMeasure.Each });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UOMs = new SelectList(Enum.GetValues<UnitOfMeasure>());
                return View(model);
            }
            try
            {
                var product = await _mfgService.CreateProductAsync(model);
                TempData["Success"] = $"Product '{product.ProductCode}' created.";
                return RedirectToAction(nameof(Products));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.UOMs = new SelectList(Enum.GetValues<UnitOfMeasure>());
                return View(model);
            }
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            try
            {
                var product = await _mfgService.GetProductByIdAsync(id);
                ViewData["Title"] = product.ProductCode;
                ViewData["Module"] = "Manufacturing";
                return View(product);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ── BOMs ─────────────────────────────

        public async Task<IActionResult> BOMs()
        {
            ViewData["Title"] = "Bills of Material";
            ViewData["Module"] = "Manufacturing";
            return View(await _mfgService.GetBOMsAsync());
        }

        public async Task<IActionResult> CreateBOM()
        {
            ViewData["Title"] = "New BOM";
            ViewData["Module"] = "Manufacturing";
            var products = await _mfgService.GetProductsAsync();
            ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
            return View(new BOMCreateVm { OutputQuantity = 1 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBOM(BOMCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                var products = await _mfgService.GetProductsAsync();
                ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
                return View(model);
            }
            try
            {
                var bom = await _mfgService.CreateBOMAsync(model);
                TempData["Success"] = $"BOM '{bom.BOMCode}' created.";
                return RedirectToAction(nameof(BOMDetail), new { id = bom.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _mfgService.GetProductsAsync();
                ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
                return View(model);
            }
        }

        public async Task<IActionResult> BOMDetail(int id)
        {
            try
            {
                var bom = await _mfgService.GetBOMByIdAsync(id);
                ViewData["Title"] = bom.BOMCode;
                ViewData["Module"] = "Manufacturing";
                var products = await _mfgService.GetProductsAsync();
                ViewBag.Components = new SelectList(products, "Id", "Name");
                ViewBag.UOMs = new SelectList(Enum.GetValues<UnitOfMeasure>());
                return View(bom);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBOMLine(BOMLineCreateVm model)
        {
            try
            {
                await _mfgService.AddBOMLineAsync(model);
                TempData["Success"] = "Component added.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(BOMDetail), new { id = model.BOMId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveBOMLine(int lineId, int bomId)
        {
            try { await _mfgService.RemoveBOMLineAsync(lineId); TempData["Success"] = "Component removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(BOMDetail), new { id = bomId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateBOM(int id)
        {
            try { await _mfgService.ActivateBOMAsync(id); TempData["Success"] = "BOM activated."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(BOMDetail), new { id });
        }

        // ── Routings ─────────────────────────

        public async Task<IActionResult> Routings()
        {
            ViewData["Title"] = "Routings";
            ViewData["Module"] = "Manufacturing";
            return View(await _mfgService.GetRoutingsAsync());
        }

        public async Task<IActionResult> CreateRouting()
        {
            ViewData["Title"] = "New Routing";
            ViewData["Module"] = "Manufacturing";
            var products = await _mfgService.GetProductsAsync();
            ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
            return View(new RoutingCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRouting(RoutingCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                var products = await _mfgService.GetProductsAsync();
                ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
                return View(model);
            }
            try
            {
                var routing = await _mfgService.CreateRoutingAsync(model);
                TempData["Success"] = $"Routing '{routing.RoutingCode}' created.";
                return RedirectToAction(nameof(RoutingDetail), new { id = routing.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _mfgService.GetProductsAsync();
                ViewBag.Products = new SelectList(products.Where(p => p.IsFinishedGood), "Id", "Name");
                return View(model);
            }
        }

        public async Task<IActionResult> RoutingDetail(int id)
        {
            try
            {
                var routing = await _mfgService.GetRoutingByIdAsync(id);
                ViewData["Title"] = routing.RoutingCode;
                ViewData["Module"] = "Manufacturing";
                ViewBag.WorkCenters = new SelectList(await _mfgService.GetWorkCentersAsync(), "Id", "Name");
                return View(routing);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOperation(RoutingOperationCreateVm model)
        {
            try
            {
                await _mfgService.AddOperationAsync(model);
                TempData["Success"] = "Operation added.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(RoutingDetail), new { id = model.RoutingId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOperation(int operationId, int routingId)
        {
            try { await _mfgService.RemoveOperationAsync(operationId); TempData["Success"] = "Operation removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(RoutingDetail), new { id = routingId });
        }

        // GET: /ProductCatalog/BOMExplosion/5
        public async Task<IActionResult> BOMExplosion(int id, decimal qty = 1)
        {
            try
            {
                var result = await _mfgService.ExplodeBOMAsync(id, qty);
                ViewData["Title"] = $"BOM Explosion — {result.BOMCode}";
                ViewData["Module"] = "Manufacturing";
                ViewBag.BOMId = id;
                return View(result);
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(BOMs));
            }
        }
    }
}
