namespace MiniB2B.Application.DTOs.Cart;

public class CartDto
{
    public IReadOnlyList<CartItemDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public int TotalItemCount { get; set; }
}
