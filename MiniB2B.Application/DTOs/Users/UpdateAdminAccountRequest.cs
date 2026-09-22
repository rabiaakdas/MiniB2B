using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Users;

public class UpdateAdminAccountRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}
