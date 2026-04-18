using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Models;

namespace ProjectApprovalSystem.Controllers
{
    [Authorize(Roles = "ModuleLeader,SystemAdmin")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CoordinatorController(ApplicationDbContext context) => _context = context;
        public async Task<IActionResult> Index() => View(await _context.Matches.Include(m => m.Project).ThenInclude(p => p.Student).Include(m => m.Supervisor).ToListAsync());
        public async Task<IActionResult> ManageResearchAreas() => View(await _context.ResearchAreas.ToListAsync());
        public IActionResult CreateResearchArea() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResearchArea(ResearchArea area)
        {
            if (ModelState.IsValid) { _context.Add(area); await _context.SaveChangesAsync(); return RedirectToAction(nameof(ManageResearchAreas)); }
            return View(area);
        }
    }
}