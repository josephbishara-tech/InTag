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
    //[Authorize]
    [AllowAnonymous]
    [RequireModule(PlatformModule.Document)]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _docService;
        private readonly IAssetService _assetService;
        private readonly IDocumentFileService _fileService;


        public DocumentController(
            IDocumentService docService,
            IAssetService assetService,
            IDocumentFileService fileService)
        {
            _docService = docService;
            _assetService = assetService;
            _fileService = fileService;
        }

        // GET: /Document
        public async Task<IActionResult> Index(DocumentFilterVm filter)
        {
            ViewData["Title"] = "Documents";
            ViewData["Module"] = "Documents";

            var result = await _docService.GetAllAsync(filter);
            await PopulateFilterDropdowns();
            ViewBag.CurrentFilter = filter;

            return View(result);
        }

        // GET: /Document/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var doc = await _docService.GetByIdAsync(id);
                ViewData["Title"] = doc.DocNumber;
                ViewData["Module"] = "Documents";
                return View(doc);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // GET: /Document/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New Document";
            ViewData["Module"] = "Documents";
            await PopulateFormDropdowns();
            return View(new DocumentCreateVm
            {
                ReviewCycle = ReviewCycle.Annual,
                Type = DocumentType.SOP,
                Category = DocumentCategory.Quality
            });
        }

        // POST: /Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateFormDropdowns();
                return View(model);
            }

            try
            {
                var doc = await _docService.CreateAsync(model);
                TempData["Success"] = $"Document '{doc.DocNumber}' created successfully.";
                return RedirectToAction(nameof(Details), new { id = doc.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateFormDropdowns();
                return View(model);
            }
        }

        // GET: /Document/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var doc = await _docService.GetByIdAsync(id);
                ViewData["Title"] = $"Edit {doc.DocNumber}";
                ViewData["Module"] = "Documents";
                ViewBag.DocId = id;
                ViewBag.DocNumber = doc.DocNumber;
                await PopulateFormDropdowns();

                return View(new DocumentUpdateVm
                {
                    Title = doc.Title,
                    Description = doc.Description,
                    Category = doc.Category,
                    ReviewCycle = doc.ReviewCycle,
                    IsoReference = doc.IsoReference,
                    ConfidentialityLevel = doc.ConfidentialityLevel,
                    Tags = doc.Tags,
                    Notes = doc.Notes
                });
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // POST: /Document/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentUpdateVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DocId = id;
                await PopulateFormDropdowns();
                return View(model);
            }

            try
            {
                await _docService.UpdateAsync(id, model);
                TempData["Success"] = "Document updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.DocId = id;
                await PopulateFormDropdowns();
                return View(model);
            }
        }

        // POST: /Document/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _docService.SoftDeleteAsync(id);
                TempData["Success"] = "Document deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ── Check-in / Check-out ─────────────

        // POST: /Document/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            try
            {
                await _docService.CheckOutAsync(id);
                TempData["Success"] = "Document checked out.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Document/CheckIn/5
        public async Task<IActionResult> CheckIn(int id)
        {
            try
            {
                var doc = await _docService.GetByIdAsync(id);
                ViewData["Title"] = $"Check In — {doc.DocNumber}";
                ViewData["Module"] = "Documents";
                ViewBag.Document = doc;
                return View(new RevisionCreateVm { DocumentId = id });
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // POST: /Document/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id, RevisionCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                var doc = await _docService.GetByIdAsync(id);
                ViewBag.Document = doc;
                return View(model);
            }

            try
            {
                model.DocumentId = id;
                await _docService.CheckInAsync(id, model);
                TempData["Success"] = "Document checked in with new revision.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: /Document/CancelCheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelCheckOut(int id)
        {
            try
            {
                await _docService.CancelCheckOutAsync(id);
                TempData["Success"] = "Check-out cancelled.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Lifecycle ────────────────────────

        // POST: /Document/Publish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                await _docService.PublishAsync(id);
                TempData["Success"] = "Document published.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Document/Obsolete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Obsolete(int id)
        {
            try
            {
                await _docService.ObsoleteAsync(id);
                TempData["Success"] = "Document marked obsolete.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Document/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _docService.ArchiveAsync(id);
                TempData["Success"] = "Document archived.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Revisions & Approval ─────────────

        // POST: /Document/CreateRevision
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRevision(RevisionCreateVm model)
        {
            try
            {
                await _docService.CreateRevisionAsync(model);
                TempData["Success"] = "New revision created.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        // POST: /Document/ApproveRevision
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRevision(RevisionApprovalVm model, int documentId)
        {
            try
            {
                await _docService.ApproveRevisionAsync(model);
                TempData["Success"] = model.IsApproved ? "Revision approved." : "Revision rejected.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // ── Distribution ─────────────────────

        // POST: /Document/Distribute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Distribute(DistributionCreateVm model)
        {
            try
            {
                await _docService.DistributeAsync(model);
                TempData["Success"] = "Document distributed.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        // POST: /Document/Acknowledge/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(int distributionId, int documentId)
        {
            try
            {
                await _docService.AcknowledgeDistributionAsync(distributionId);
                TempData["Success"] = "Acknowledged.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // ── Helpers ──────────────────────────

        private async Task PopulateFilterDropdowns()
        {
            ViewBag.Departments = new SelectList(await _assetService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Statuses = new SelectList(Enum.GetValues<DocumentStatus>());
            ViewBag.Types = new SelectList(Enum.GetValues<DocumentType>());
            ViewBag.Categories = new SelectList(Enum.GetValues<DocumentCategory>());
        }

        private async Task PopulateFormDropdowns()
        {
            ViewBag.Departments = new SelectList(await _assetService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Types = new SelectList(Enum.GetValues<DocumentType>());
            ViewBag.Categories = new SelectList(Enum.GetValues<DocumentCategory>());
            ViewBag.ReviewCycles = new SelectList(Enum.GetValues<ReviewCycle>());
        }

        // -- File upload/download actions would go here, but are omitted for brevity.

        // POST: /Document/UploadFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int revisionId, int documentId, IFormFile file)
        {
            try
            {
                var result = await _fileService.UploadFileAsync(revisionId, file);
                TempData["Success"] = $"File '{result.FileName}' uploaded ({result.FileSize / 1024} KB).";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // GET: /Document/DownloadFile/5
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var result = await _fileService.DownloadFileAsync(fileId);
                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Document/DeleteFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId, int documentId)
        {
            try
            {
                await _fileService.DeleteFileAsync(fileId);
                TempData["Success"] = "File deleted.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // GET: /Document/Search
        public async Task<IActionResult> Search(string q, int page = 1)
        {
            ViewData["Title"] = "Document Search";
            ViewData["Module"] = "Documents";
            ViewBag.SearchTerm = q;

            if (string.IsNullOrWhiteSpace(q))
                return View(new DocumentSearchResultVm());

            var result = await _fileService.FullTextSearchAsync(q, page, 25);
            return View(result);
        }


    }
}
