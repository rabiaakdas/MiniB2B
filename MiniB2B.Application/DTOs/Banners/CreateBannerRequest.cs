using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Banners;

public class CreateBannerRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Subtitle { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
