using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Cart;

public class UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
