using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectApprovalSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectApprovalSystem.Pages
{
    [Authorize]
    public class CompleteProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public CompleteProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        { _userManager = userManager; _signInManager = signInManager; }
        [BindProperty][Required] public string FullName { get; set; } = string.Empty;
        [BindProperty][Required] public string SelectedRole { get; set; } = string.Empty;
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!string.IsNullOrEmpty(user.FullName) && (await _userManager.GetRolesAsync(user)).Any()) return RedirectToPage("/Index");
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });
            user.FullName = FullName;
            await _userManager.UpdateAsync(user);
            if (!string.IsNullOrEmpty(SelectedRole)) await _userManager.AddToRoleAsync(user, SelectedRole);
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage("/Index");
        }
    }
}