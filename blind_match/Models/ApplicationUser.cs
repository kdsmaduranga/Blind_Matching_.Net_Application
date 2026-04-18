using Microsoft.AspNetCore.Identity;

namespace ProjectApprovalSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; } = string.Empty;
        public ICollection<Project> SubmittedProjects { get; set; } = new List<Project>();
        public ICollection<Match> MatchesAsSupervisor { get; set; } = new List<Match>();
    }
}