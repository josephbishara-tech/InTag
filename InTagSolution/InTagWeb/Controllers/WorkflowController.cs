using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Workflow;
using InTagViewModelLayer.Workflow;
using InTagWeb.Filters;

namespace InTagWeb.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [RequireModule(PlatformModule.Workflow)]
    public class WorkflowController : Controller
    {
        private readonly IWorkflowService _wfService;
        private readonly INotificationService _notifService;

        public WorkflowController(IWorkflowService wfService, INotificationService notifService)
        {
            _wfService = wfService;
            _notifService = notifService;
        }

        // ── Definitions ──────────────────────

        public async Task<IActionResult> Definitions()
        {
            ViewData["Title"] = "Workflow Definitions";
            ViewData["Module"] = "Workflow";
            return View(await _wfService.GetDefinitionsAsync());
        }

        public IActionResult CreateDefinition()
        {
            ViewData["Title"] = "New Workflow";
            ViewData["Module"] = "Workflow";
            PopulateDefinitionDropdowns();
            return View(new WorkflowDefinitionCreateVm { Category = WorkflowCategory.General });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDefinition(WorkflowDefinitionCreateVm model)
        {
            if (!ModelState.IsValid) { PopulateDefinitionDropdowns(); return View(model); }
            try
            {
                var def = await _wfService.CreateDefinitionAsync(model);
                TempData["Success"] = $"Workflow '{def.Name}' created.";
                return RedirectToAction(nameof(DefinitionDetail), new { id = def.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                PopulateDefinitionDropdowns();
                return View(model);
            }
        }

        public async Task<IActionResult> DefinitionDetail(int id)
        {
            try
            {
                var def = await _wfService.GetDefinitionByIdAsync(id);
                ViewData["Title"] = def.Name;
                ViewData["Module"] = "Workflow";
                PopulateStepDropdowns();
                return View(def);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStep(WorkflowStepCreateVm model)
        {
            try
            {
                await _wfService.AddStepAsync(model);
                TempData["Success"] = "Step added.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(DefinitionDetail), new { id = model.WorkflowDefinitionId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStep(int stepId, int definitionId)
        {
            try { await _wfService.RemoveStepAsync(stepId); TempData["Success"] = "Step removed."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(DefinitionDetail), new { id = definitionId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDefinition(int id)
        {
            try { await _wfService.ActivateDefinitionAsync(id); TempData["Success"] = "Workflow activated."; }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(DefinitionDetail), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateDefinition(int id)
        {
            try { await _wfService.DeactivateDefinitionAsync(id); TempData["Success"] = "Workflow deactivated."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(DefinitionDetail), new { id });
        }

        // ── Instances ────────────────────────

        public async Task<IActionResult> Index(WorkflowFilterVm filter)
        {
            ViewData["Title"] = "Workflow Instances";
            ViewData["Module"] = "Workflow";
            var result = await _wfService.GetInstancesAsync(filter);
            ViewBag.CurrentFilter = filter;
            PopulateInstanceDropdowns();
            return View(result);
        }

        public async Task<IActionResult> Instance(int id)
        {
            try
            {
                var inst = await _wfService.GetInstanceByIdAsync(id);
                ViewData["Title"] = inst.InstanceNumber;
                ViewData["Module"] = "Workflow";
                return View(inst);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        public async Task<IActionResult> StartWorkflow()
        {
            ViewData["Title"] = "Start Workflow";
            ViewData["Module"] = "Workflow";
            var defs = await _wfService.GetDefinitionsAsync();
            ViewBag.Definitions = new SelectList(
                defs.Where(d => d.Status == WorkflowStatus.Active), "Id", "Name");
            return View(new StartWorkflowVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWorkflow(StartWorkflowVm model)
        {
            if (!ModelState.IsValid)
            {
                var defs = await _wfService.GetDefinitionsAsync();
                ViewBag.Definitions = new SelectList(
                    defs.Where(d => d.Status == WorkflowStatus.Active), "Id", "Name");
                return View(model);
            }
            try
            {
                var inst = await _wfService.StartWorkflowAsync(model);
                TempData["Success"] = $"Workflow '{inst.InstanceNumber}' started.";
                return RedirectToAction(nameof(Instance), new { id = inst.Id });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                var defs = await _wfService.GetDefinitionsAsync();
                ViewBag.Definitions = new SelectList(
                    defs.Where(d => d.Status == WorkflowStatus.Active), "Id", "Name");
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAction(WorkflowActionSubmitVm model)
        {
            try
            {
                await _wfService.SubmitActionAsync(model);
                TempData["Success"] = $"Action '{model.Action}' recorded.";
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Instance), new { id = model.WorkflowInstanceId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelInstance(int id)
        {
            try { await _wfService.CancelInstanceAsync(id); TempData["Success"] = "Workflow cancelled."; }
            catch (KeyNotFoundException) { TempData["Error"] = "Not found."; }
            return RedirectToAction(nameof(Instance), new { id });
        }

        // ── Pending Approvals ────────────────

        public async Task<IActionResult> PendingApprovals()
        {
            ViewData["Title"] = "Pending Approvals";
            ViewData["Module"] = "Workflow";
            return View(await _wfService.GetPendingApprovalsAsync());
        }

        // ── Notifications ────────────────────

        public async Task<IActionResult> Notifications()
        {
            ViewData["Title"] = "Notifications";
            ViewData["Module"] = "Workflow";
            var userId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            var notifications = await _notifService.GetForUserAsync(Guid.Parse(userId));
            return View(notifications);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int notificationId)
        {
            await _notifService.MarkAsReadAsync(notificationId);
            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null) await _notifService.MarkAllReadAsync(Guid.Parse(userId));
            return RedirectToAction(nameof(Notifications));
        }

        // ── Helpers ──────────────────────────

        private void PopulateDefinitionDropdowns()
        {
            ViewBag.Categories = new SelectList(Enum.GetValues<WorkflowCategory>());
        }

        private void PopulateStepDropdowns()
        {
            ViewBag.StepTypes = new SelectList(Enum.GetValues<StepType>());
            ViewBag.ExecutionModes = new SelectList(Enum.GetValues<StepExecutionMode>());
            ViewBag.NotificationChannels = new SelectList(Enum.GetValues<NotificationChannel>());
        }

        private void PopulateInstanceDropdowns()
        {
            ViewBag.Statuses = new SelectList(Enum.GetValues<WorkflowInstanceStatus>());
            ViewBag.Categories = new SelectList(Enum.GetValues<WorkflowCategory>());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RunEscalations()
        {
            var result = await _wfService.ProcessEscalationsAsync();
            if (result.EscalatedCount > 0)
                TempData["Success"] = $"{result.EscalatedCount} workflow(s) escalated out of {result.ProcessedCount} active.";
            else
                TempData["Success"] = $"No escalations needed. {result.ProcessedCount} active workflows checked.";
            ViewData["Title"] = "Escalation Results";
            ViewData["Module"] = "Workflow";
            return View("EscalationResult", result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedTemplates()
        {
            var count = await _wfService.SeedTemplatesAsync();
            TempData["Success"] = count > 0 ? $"{count} workflow template(s) seeded." : "All templates already exist.";
            return RedirectToAction(nameof(Definitions));
        }
    }
}
