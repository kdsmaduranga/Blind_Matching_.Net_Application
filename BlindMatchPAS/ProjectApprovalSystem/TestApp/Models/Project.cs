using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectApprovalSystem.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        [Required][DataType(DataType.MultilineText)] public string Abstract { get; set; } = string.Empty;
        [Required] public string TechnicalStack { get; set; } = string.Empty;
        public int ResearchAreaId { get; set; }
        [ForeignKey("ResearchAreaId")] public ResearchArea? ResearchArea { get; set; }
        public string StudentId { get; set; } = string.Empty;
        [ForeignKey("StudentId")] public ApplicationUser? Student { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
