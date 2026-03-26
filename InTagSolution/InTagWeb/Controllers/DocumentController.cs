using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagLogicLayer.Document;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Document;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
    [RequireModule(PlatformModule.Document)]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _docService;
        private readonly IAssetLookupService _lookupService;
        private readonly IDocumentFileService _fileService;
        private readonly IUnitOfWork _uow;

        public DocumentController(
            IDocumentService docService,
            IAssetLookupService lookupService,
            IDocumentFileService fileService,
            IUnitOfWork uow)
        {
            _docService = docService;
            _lookupService = lookupService;
            _fileService = fileService;
            _uow = uow;
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateVm model)
        {
            if (!ModelState.IsValid) { await PopulateFormDropdowns(); return View(model); }
            try
            {
                var doc = await _docService.CreateAsync(model);
                TempData["Success"] = $"Document '{doc.DocNumber}' created.";
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentUpdateVm model)
        {
            if (!ModelState.IsValid) { ViewBag.DocId = id; await PopulateFormDropdowns(); return View(model); }
            try
            {
                await _docService.UpdateAsync(id, model);
                TempData["Success"] = "Document updated.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.DocId = id; await PopulateFormDropdowns();
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _docService.SoftDeleteAsync(id); TempData["Success"] = "Document deleted."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; return RedirectToAction(nameof(Details), new { id }); }
        }

        // ── Check-in / Check-out ─────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            try { await _docService.CheckOutAsync(id); TempData["Success"] = "Document checked out. Download the files below to begin editing."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

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

        [HttpPost, ValidateAntiForgeryToken]
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
            { TempData["Error"] = ex.Message; return RedirectToAction(nameof(Details), new { id }); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelCheckOut(int id)
        {
            try { await _docService.CancelCheckOutAsync(id); TempData["Success"] = "Check-out cancelled."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Lifecycle ────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try { await _docService.PublishAsync(id); TempData["Success"] = "Document published."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Obsolete(int id)
        {
            try { await _docService.ObsoleteAsync(id); TempData["Success"] = "Document marked obsolete."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            try { await _docService.ArchiveAsync(id); TempData["Success"] = "Document archived."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Revisions & Approval ─────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRevision(RevisionCreateVm model)
        {
            try { await _docService.CreateRevisionAsync(model); TempData["Success"] = "New revision created."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        [HttpPost, ValidateAntiForgeryToken]
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Distribute(DistributionCreateVm model)
        {
            try { await _docService.DistributeAsync(model); TempData["Success"] = "Document distributed."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(int distributionId, int documentId)
        {
            try { await _docService.AcknowledgeDistributionAsync(distributionId); TempData["Success"] = "Acknowledged."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // ── File Upload / Download ───────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int revisionId, int documentId, IFormFile file)
        {
            try
            {
                var result = await _fileService.UploadFileAsync(revisionId, file);
                TempData["Success"] = $"File '{result.FileName}' uploaded ({result.FileSize / 1024} KB).";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // GET: /Document/DownloadFile?fileId=5
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var result = await _fileService.DownloadFileAsync(fileId);
                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // GET: /Document/DownloadRevisionFiles?revisionId=5
        public async Task<IActionResult> DownloadRevisionFiles(int revisionId)
        {
            var files = await _uow.DocumentFiles.Query()
                .Where(f => f.RevisionId == revisionId)
                .ToListAsync();

            if (!files.Any()) return NotFound("No files in this revision.");

            // Single file — download directly
            if (files.Count == 1)
            {
                var result = await _fileService.DownloadFileAsync(files.First().Id);
                return File(result.Content, result.ContentType, result.FileName);
            }

            // Multiple files — ZIP
            var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    var download = await _fileService.DownloadFileAsync(file.Id);
                    var entry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    await download.Content.CopyToAsync(entryStream);
                }
            }
            zipStream.Position = 0;
            return File(zipStream, "application/zip", $"revision_{revisionId}_files.zip");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId, int documentId)
        {
            try { await _fileService.DeleteFileAsync(fileId); TempData["Success"] = "File deleted."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        // GET: /Document/Search
        public async Task<IActionResult> Search(string q, int page = 1)
        {
            ViewData["Title"] = "Document Search";
            ViewData["Module"] = "Documents";
            ViewBag.SearchTerm = q;
            if (string.IsNullOrWhiteSpace(q)) return View(new DocumentSearchResultVm());
            var result = await _fileService.FullTextSearchAsync(q, page, 25);
            return View(result);
        }

        // ── Helpers ──────────────────────────

        private async Task PopulateFilterDropdowns()
        {
            ViewBag.Departments = new SelectList(await _lookupService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Statuses = new SelectList(Enum.GetValues<DocumentStatus>());
            ViewBag.Types = new SelectList(Enum.GetValues<DocumentType>());
            ViewBag.Categories = new SelectList(Enum.GetValues<DocumentCategory>());
        }

        private async Task PopulateFormDropdowns()
        {
            ViewBag.Departments = new SelectList(await _lookupService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Types = new SelectList(Enum.GetValues<DocumentType>());
            ViewBag.Categories = new SelectList(Enum.GetValues<DocumentCategory>());
            ViewBag.ReviewCycles = new SelectList(Enum.GetValues<ReviewCycle>());
        }
    }
}
