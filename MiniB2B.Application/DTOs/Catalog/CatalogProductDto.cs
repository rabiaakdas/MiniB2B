namespace MiniB2B.Application.DTOs.Catalog;

public class CatalogProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
