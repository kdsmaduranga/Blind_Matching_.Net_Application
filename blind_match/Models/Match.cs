using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectApprovalSystem.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")] public Project? Project { get; set; }
        public string SupervisorId { get; set; } = string.Empty;
        [ForeignKey("SupervisorId")] public ApplicationUser? Supervisor { get; set; }
        public DateTime MatchDate { get; set; } = DateTime.UtcNow;
    }
}