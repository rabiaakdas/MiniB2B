namespace MiniB2B.Application.DTOs.Products;

public class ProductListItemDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? SpecialCode1 { get; set; }
    public string? SpecialCode2 { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
