using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Models;

namespace ProjectApprovalSystem.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        { _context = context; _userManager = userManager; }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            IQueryable<Project> projects = _context.Projects.Include(p => p.ResearchArea).Include(p => p.Student);
            if (User.IsInRole("Student")) projects = projects.Where(p => p.StudentId == currentUser.Id);
            return View(await projects.ToListAsync());
        }

        [Authorize(Roles = "Student")]
        public IActionResult Create()
        {
            ViewData["ResearchAreaId"] = new SelectList(_context.ResearchAreas, "Id", "Name");
            return View();
        }

        [HttpPost][ValidateAntiForgeryToken][Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([Bind("Title,Abstract,TechnicalStack,ResearchAreaId")] Project project)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            project.StudentId = currentUser.Id;
            ModelState.Remove("Student"); ModelState.Remove("ResearchArea");
            if (ModelState.IsValid) { _context.Add(project); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
            ViewData["ResearchAreaId"] = new SelectList(_context.ResearchAreas, "Id", "Name", project.ResearchAreaId);
            return View(project);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> BlindReview()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var matched = await _context.Matches.Where(m => m.SupervisorId == currentUser.Id).Select(m => m.ProjectId).ToListAsync();
            var available = await _context.Projects.Include(p => p.ResearchArea)
                .Where(p => p.Status == "Pending" && !matched.Contains(p.Id))
                .Select(p => new BlindProjectViewModel { Id = p.Id, Title = p.Title, Abstract = p.Abstract, TechnicalStack = p.TechnicalStack, ResearchAreaName = p.ResearchArea.Name, Status = p.Status })
                .ToListAsync();
            return View(available);
        }

        [HttpPost][ValidateAntiForgeryToken][Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ExpressInterest(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.Status != "Pending") return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            if (await _context.Matches.AnyAsync(m => m.ProjectId == id)) return BadRequest("Already matched.");
            var match = new Match { ProjectId = id, SupervisorId = currentUser.Id };
            project.Status = "Matched";
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Matched!";
            return RedirectToAction(nameof(BlindReview));
        }
    }
}
