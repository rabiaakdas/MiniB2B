using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Users;

public class ChangeAdminPasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
