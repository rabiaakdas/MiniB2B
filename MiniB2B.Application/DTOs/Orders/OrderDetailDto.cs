namespace MiniB2B.Application.DTOs.Orders;

public class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public IReadOnlyList<OrderItemDto> Items { get; set; } = [];
}
