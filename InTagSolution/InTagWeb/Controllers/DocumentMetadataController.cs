using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Document;
using InTagViewModelLayer.Document;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
    [RequireModule(PlatformModule.Document)]
    public class DocumentMetadataController : Controller
    {
        private readonly IDocumentMetadataService _metaService;
        private readonly IDocumentService _docService;

        public DocumentMetadataController(IDocumentMetadataService metaService, IDocumentService docService)
        {
            _metaService = metaService;
            _docService = docService;
        }

        // ── Templates ────────────────────────

        public async Task<IActionResult> Templates()
        {
            ViewData["Title"] = "Metadata Templates";
            ViewData["Module"] = "Documents";
            return View(await _metaService.GetTemplatesAsync());
        }

        public IActionResult CreateTemplate()
        {
            ViewData["Title"] = "New Template";
            ViewData["Module"] = "Documents";
            ViewBag.DocTypes = new SelectList(Enum.GetValues<DocumentType>());
            return View(new MetadataTemplateCreateVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTemplate(MetadataTemplateCreateVm model)
        {
            if (!ModelState.IsValid) { ViewBag.DocTypes = new SelectList(Enum.GetValues<DocumentType>()); return View(model); }
            try
            {
                var t = await _metaService.CreateTemplateAsync(model);
                TempData["Success"] = $"Template '{t.Name}' created.";
                return RedirectToAction(nameof(TemplateDetail), new { id = t.Id });
            }
            catch (InvalidOperationException ex)
            { ModelState.AddModelError("", ex.Message); ViewBag.DocTypes = new SelectList(Enum.GetValues<DocumentType>()); return View(model); }
        }

        public async Task<IActionResult> TemplateDetail(int id)
        {
            try
            {
                var t = await _metaService.GetTemplateByIdAsync(id);
                ViewData["Title"] = t.Name;
                ViewData["Module"] = "Documents";
                ViewBag.FieldTypes = new SelectList(Enum.GetValues<MetadataFieldType>());
                return View(t);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddField(FieldDefinitionCreateVm model)
        {
            try
            {
                await _metaService.AddFieldToTemplateAsync(model);
                TempData["Success"] = $"Field '{model.FieldName}' added.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(TemplateDetail), new { id = model.TemplateId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveField(int fieldId, int templateId)
        {
            try { await _metaService.RemoveFieldAsync(fieldId); TempData["Success"] = "Field removed."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(TemplateDetail), new { id = templateId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try { await _metaService.DeleteTemplateAsync(id); TempData["Success"] = "Template deleted."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Templates));
        }

        // ── Metadata Editor ──────────────────

        public async Task<IActionResult> Edit(int? documentId, int? userFileId, int? userFolderId)
        {
            ViewData["Title"] = "Edit Metadata";
            ViewData["Module"] = "Documents";
            var vm = await _metaService.GetMetadataAsync(documentId, userFileId, userFolderId);
            ViewBag.Templates = new SelectList(await _metaService.GetTemplatesAsync(), "Id", "Name");
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MetadataEditVm model)
        {
            await _metaService.SaveMetadataAsync(model);
            TempData["Success"] = "Metadata saved.";

            if (model.DocumentId.HasValue)
                return RedirectToAction("Details", "Document", new { id = model.DocumentId });
            if (model.UserFileId.HasValue)
                return RedirectToAction("Index", "DocumentRepository");
            return RedirectToAction("Index", "DocumentRepository");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyTemplate(int templateId, int? documentId, int? userFileId, int? userFolderId)
        {
            try
            {
                await _metaService.ApplyTemplateAsync(templateId, documentId, userFileId, userFolderId);
                TempData["Success"] = "Template applied.";
            }
            catch (KeyNotFoundException) { TempData["Error"] = "Template not found."; }
            return RedirectToAction(nameof(Edit), new { documentId, userFileId, userFolderId });
        }

        // ── Tags ─────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTag(TagAddVm model, string? returnUrl)
        {
            try { await _metaService.AddTagAsync(model); TempData["Success"] = $"Tag '{model.Tag}' added."; }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            if (model.DocumentId.HasValue) return RedirectToAction("Details", "Document", new { id = model.DocumentId });
            return RedirectToAction("Index", "DocumentRepository");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTag(int tagId, int? documentId, string? returnUrl)
        {
            try { await _metaService.RemoveTagAsync(tagId); TempData["Success"] = "Tag removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            if (documentId.HasValue) return RedirectToAction("Details", "Document", new { id = documentId });
            return RedirectToAction("Index", "DocumentRepository");
        }

        // ── Links ────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLink(DocumentLinkCreateVm model)
        {
            try { await _metaService.AddLinkAsync(model); TempData["Success"] = "Link added."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction("Details", "Document", new { id = model.SourceDocumentId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLink(int linkId, int documentId)
        {
            try { await _metaService.RemoveLinkAsync(linkId); TempData["Success"] = "Link removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction("Details", "Document", new { id = documentId });
        }
    }
}
