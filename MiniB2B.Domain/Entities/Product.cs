namespace MiniB2B.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? SpecialCode1 { get; set; }
    public string? SpecialCode2 { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Category Category { get; set; } = null!;
}
