using System.ComponentModel.DataAnnotations;

namespace ProjectApprovalSystem.Models
{
    public class RegisterViewModel
    {
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string FullName { get; set; } = string.Empty;
        [Required][StringLength(100, MinimumLength = 6)][DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        [DataType(DataType.Password)][Compare("Password")] public string ConfirmPassword { get; set; } = string.Empty;
        [Required] public string SelectedRole { get; set; } = string.Empty;
    }
}
