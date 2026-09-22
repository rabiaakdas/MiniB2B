namespace MiniB2B.Application.DTOs.Cart;

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int StockQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
