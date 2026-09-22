using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Orders;

namespace MiniB2B.Application.Interfaces;

public interface IAdminOrderService
{
    Task<PagedResponse<AdminOrderListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? status);

    Task<ServiceResult<AdminOrderDetailDto>> GetByIdAsync(int id);
    Task<ServiceResult<AdminOrderDetailDto>> UpdateStatusAsync(int id, UpdateOrderStatusRequest request);
}
