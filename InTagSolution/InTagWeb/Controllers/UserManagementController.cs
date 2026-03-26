using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InTagEntitiesLayer.Common;
using InTagViewModelLayer.Identity;

namespace InTagWeb.Controllers
{
     [Authorize(Roles = "Admin")]
    //[AllowAnonymous]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "User Management";
            var users = await _userManager.Users.OrderBy(u => u.LastName).ToListAsync();

            var userVms = new List<UserProfileVm>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userVms.Add(new UserProfileVm
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = roles,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate
                });
            }
            return View(userVms);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "New User";
            PopulateDropdowns();
            return View(new CreateUserVm { Role = "Operator" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm model)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(model); }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                PopulateDropdowns();
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                TenantId = currentUser.TenantId,
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                PopulateDropdowns();
                return View(model);
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(model.Role));

            await _userManager.AddToRoleAsync(user, model.Role);

            // Add TenantId claim
            await _userManager.AddClaimAsync(user,
                new System.Security.Claims.Claim("TenantId", currentUser.TenantId.ToString()));

            TempData["Success"] = $"User '{user.FirstName} {user.LastName}' created with role '{model.Role}'.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            ViewData["Title"] = $"Edit — {user.FirstName} {user.LastName}";
            var roles = await _userManager.GetRolesAsync(user);
            PopulateDropdowns();

            return View(new EditUserVm
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Operator",
                IsActive = user.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVm model)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(model); }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                PopulateDropdowns();
                return View(model);
            }

            // Update role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(model.Role));
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = $"User '{user.FirstName} {user.LastName}' updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsActive
                ? $"User '{user.FirstName} {user.LastName}' activated."
                : $"User '{user.FirstName} {user.LastName}' deactivated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            ViewData["Title"] = $"Reset Password — {user.FirstName} {user.LastName}";
            return View(new ResetPasswordVm { UserId = id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }

            TempData["Success"] = $"Password reset for '{user.FirstName} {user.LastName}'.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Roles = new SelectList(new[] { "Admin", "Manager", "Operator", "Viewer" });
        }
    }
}
