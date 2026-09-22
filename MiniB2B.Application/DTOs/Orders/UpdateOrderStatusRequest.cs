using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Application.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
