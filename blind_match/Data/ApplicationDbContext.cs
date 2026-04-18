using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Models;

namespace ProjectApprovalSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<Match> Matches { get; set; }
    }
}