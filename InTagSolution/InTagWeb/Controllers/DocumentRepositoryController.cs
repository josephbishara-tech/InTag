using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagLogicLayer.Document;
using InTagViewModelLayer.Document;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
    [RequireModule(PlatformModule.Document)]
    public class DocumentRepositoryController : Controller
    {
        private readonly IUserRepositoryService _repoService;
        private readonly IAssetLookupService _lookupService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentRepositoryController(IUserRepositoryService repoService,
            IAssetLookupService lookupService, UserManager<ApplicationUser> userManager)
        {
            _repoService = repoService;
            _lookupService = lookupService;
            _userManager = userManager;
        }

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? throw new UnauthorizedAccessException();
        }

        // ── My Documents ─────────────────────

        public async Task<IActionResult> Index(int? folderId)
        {
            ViewData["Title"] = "My Documents";
            ViewData["Module"] = "Documents";
            var userId = await GetCurrentUserIdAsync();
            var user = await _userManager.GetUserAsync(User);
            await _repoService.EnsureUserRootFolderAsync(userId, $"{user?.FirstName} {user?.LastName}");
            var vm = await _repoService.BrowseMyDocumentsAsync(userId, folderId);
            return View(vm);
        }

        // ── Shared With Me ───────────────────

        public async Task<IActionResult> SharedWithMe()
        {
            ViewData["Title"] = "Shared With Me";
            ViewData["Module"] = "Documents";
            var userId = await GetCurrentUserIdAsync();
            var vm = await _repoService.BrowseSharedWithMeAsync(userId);
            return View("Index", vm);
        }

        // ── Department Documents ─────────────

        public async Task<IActionResult> Department(int departmentId, int? folderId)
        {
            ViewData["Title"] = "Department Documents";
            ViewData["Module"] = "Documents";
            try
            {
                var vm = await _repoService.BrowseDepartmentAsync(departmentId, folderId);
                return View("Index", vm);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ── Create Folder ────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolder(FolderCreateVm model)
        {
            if (!ModelState.IsValid)
            { TempData["Error"] = "Folder name is required."; return RedirectToAction(nameof(Index), new { folderId = model.ParentFolderId }); }

            try
            {
                var userId = await GetCurrentUserIdAsync();
                await _repoService.CreateFolderAsync(userId, model);
                TempData["Success"] = $"Folder '{model.Name}' created.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Index), new { folderId = model.ParentFolderId });
        }

        // ── Delete Folder ────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFolder(int folderId, int? parentFolderId)
        {
            try
            {
                await _repoService.DeleteFolderAsync(folderId);
                TempData["Success"] = "Folder deleted.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
        }

        // ── Upload File ──────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int folderId, IFormFile file, string? description)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var result = await _repoService.UploadFileAsync(userId, folderId, file, description);
                TempData["Success"] = $"'{result.FileName}' uploaded ({result.FileSizeDisplay}).";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index), new { folderId });
        }

        // ── Download File ────────────────────

        public async Task<IActionResult> Download(int fileId)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var (content, fileName, contentType) = await _repoService.DownloadFileAsync(fileId, userId);
                return File(content, contentType, fileName);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // ── Delete File ──────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId, int folderId)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                await _repoService.DeleteFileAsync(fileId, userId);
                TempData["Success"] = "File deleted.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or UnauthorizedAccessException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index), new { folderId });
        }

        // ── Share File ───────────────────────

        public async Task<IActionResult> Share(int fileId)
        {
            ViewData["Title"] = "Share File";
            ViewData["Module"] = "Documents";
            var shares = await _repoService.GetFileSharesAsync(fileId);
            ViewBag.FileId = fileId;
            ViewBag.ExistingShares = shares;
            ViewBag.Departments = new SelectList(await _lookupService.GetDepartmentsAsync(), "Id", "Name");
            ViewBag.Users = new SelectList(
                _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.LastName)
                    .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName }), "Id", "Name");
            return View(new FileShareCreateVm { UserFileId = fileId, Permission = SharePermission.View });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(FileShareCreateVm model)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                await _repoService.ShareFileAsync(userId, model);
                TempData["Success"] = "File shared.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or UnauthorizedAccessException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Share), new { fileId = model.UserFileId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveShare(int shareId, int fileId)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                await _repoService.RemoveShareAsync(shareId, userId);
                TempData["Success"] = "Share removed.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or UnauthorizedAccessException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Share), new { fileId });
        }
    }
}
