using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Products;

public class CreateProductRequest
{
    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(100)]
    public string? ManufacturerCode { get; set; }

    [MaxLength(100)]
    public string? SpecialCode1 { get; set; }

    [MaxLength(100)]
    public string? SpecialCode2 { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int CriticalStockLevel { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
