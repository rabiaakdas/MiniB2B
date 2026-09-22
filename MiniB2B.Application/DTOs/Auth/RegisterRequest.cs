using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;
}
