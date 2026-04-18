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
        private readonly IWebHostEnvironment _environment;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Projects (My Projects for Student)
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            IQueryable<Project> projects = _context.Projects
                .Include(p => p.ResearchArea)
                .Include(p => p.Student);

            if (User.IsInRole("Student"))
            {
                projects = projects.Where(p => p.StudentId == currentUser.Id);
            }

            return View(await projects.ToListAsync());
        }

        // GET: Projects/Details/5 (with Reveal logic)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ResearchArea)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Students can only see their own projects, unless it's a supervisor viewing a matched project
            if (User.IsInRole("Student") && project.StudentId != currentUser.Id)
                return Forbid();

            // For students: Reveal supervisor details only if project is Matched
            if (User.IsInRole("Student") && project.Status == "Matched")
            {
                var match = await _context.Matches
                    .Include(m => m.Supervisor)
                    .FirstOrDefaultAsync(m => m.ProjectId == id);
                ViewBag.Supervisor = match?.Supervisor;
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Student")]
        public IActionResult Create()
        {
            ViewData["ResearchAreaId"] = new SelectList(_context.ResearchAreas, "Id", "Name");
            return View();
        }

        // POST: Projects/Create (with file upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(
            [Bind("Title,Abstract,TechnicalStack,ResearchAreaId")] Project project,
            IFormFile? proposalFile)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            project.StudentId = currentUser.Id;
            project.Status = "Pending";

            // Handle file upload
            if (proposalFile != null && proposalFile.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".pdf" };
                var extension = Path.GetExtension(proposalFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProposalFile", "Only PDF files are allowed.");
                }
                else if (proposalFile.Length > 5 * 1024 * 1024) // 5 MB limit
                {
                    ModelState.AddModelError("ProposalFile", "File size cannot exceed 5 MB.");
                }
                else
                {
                    // Save file
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "proposals");
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await proposalFile.CopyToAsync(stream);
                    }
                    project.ProposalFile = $"/uploads/proposals/{uniqueFileName}";
                }
            }

            // Remove navigation properties from model validation
            ModelState.Remove("Student");
            ModelState.Remove("ResearchArea");

            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ResearchAreaId"] = new SelectList(_context.ResearchAreas, "Id", "Name", project.ResearchAreaId);
            return View(project);
        }

        // GET: Projects/Edit/5 (only if Pending)
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (project == null || project.StudentId != currentUser.Id)
                return Forbid();

            if (project.Status != "Pending")
                return BadRequest("Cannot edit a project that is already under review or matched.");

            ViewData["ResearchAreaId"] = new SelectList(_context.ResearchAreas, "Id", "Name", project.ResearchAreaId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Title,Abstract,TechnicalStack,ResearchAreaId")] Project project,
            IFormFile? proposalFile)
        {
            if (id != project.Id) return NotFound();

            var dbProject = await _context.Projects.FindAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (dbProject == null || dbProject.StudentId != currentUser.Id || dbProject.Status != "Pending")
                return Forbid();

            // Handle file upload (replace old file)
            if (proposalFile != null && proposalFile.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf" };
                var extension = Path.GetExtension(proposalFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProposalFile", "Only PDF files are allowed.");
                }
                else if (proposalFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProposalFile", "File size cannot exceed 5 MB.");
                }
                else
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(dbProject.ProposalFile))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, dbProject.ProposalFile.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "proposals");
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await proposalFile.CopyToAsync(stream);
                    }
                    dbProject.ProposalFile = $"/uploads/proposals/{uniqueFileName}";
                }
            }

            ModelState.Remove("Student");
            ModelState.Remove("ResearchArea");

            if (ModelState.IsValid)
            {
                dbProject.Title = project.Title;
                dbProject.Abstract = project.Abstract;
                dbProject.TechnicalStack = project.TechnicalStack;
                dbProject.ResearchAreaId = project.ResearchAreaId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ResearchAreaId"] = new SelectList(_context.ResearchAreas, "Id", "Name", dbProject.ResearchAreaId);
            return View(dbProject);
        }

        // GET: Projects/Delete/5 (Withdraw)
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(m => m.Id == id);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (project == null || project.StudentId != currentUser.Id || project.Status != "Pending")
                return Forbid();

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (project != null && project.StudentId == currentUser.Id && project.Status == "Pending")
            {
                // Delete associated file
                if (!string.IsNullOrEmpty(project.ProposalFile))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, project.ProposalFile.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ... BlindReview and ExpressInterest methods remain unchanged ...
    }
}