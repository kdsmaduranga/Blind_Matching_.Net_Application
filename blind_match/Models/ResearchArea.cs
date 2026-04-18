using System.ComponentModel.DataAnnotations;

namespace ProjectApprovalSystem.Models
{
    public class ResearchArea
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}