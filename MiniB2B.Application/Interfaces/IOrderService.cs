using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Orders;

namespace MiniB2B.Application.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<CreateOrderResponseDto>> CreateOrderAsync(int userId);
    Task<PagedResponse<OrderListItemDto>> GetUserOrdersAsync(int userId, int page, int pageSize);
    Task<ServiceResult<OrderDetailDto>> GetUserOrderByIdAsync(int userId, int orderId);
}
