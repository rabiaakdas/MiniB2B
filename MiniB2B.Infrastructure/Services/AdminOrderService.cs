using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Orders;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Enums;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class AdminOrderService : IAdminOrderService
{
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;

    public AdminOrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<AdminOrderListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? status)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query =
            from order in _dbContext.Orders.AsNoTracking()
            join user in _dbContext.Users.AsNoTracking() on order.UserId equals user.Id
            select new
            {
                Order = order,
                User = user
            };
        var trimmedSearch = search?.Trim();

        if (!string.IsNullOrWhiteSpace(trimmedSearch))
        {
            query = query.Where(item =>
                item.Order.OrderNumber.Contains(trimmedSearch) ||
                item.User.FirstName.Contains(trimmedSearch) ||
                item.User.LastName.Contains(trimmedSearch) ||
                item.User.Email!.Contains(trimmedSearch));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (TryParseStatus(status, allowPending: true, out var parsedStatus))
            {
                query = query.Where(item => item.Order.Status == parsedStatus);
            }
            else
            {
                query = query.Where(item => false);
            }
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(item => item.Order.OrderDate)
            .ThenByDescending(item => item.Order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new AdminOrderListItemDto
            {
                Id = item.Order.Id,
                OrderNumber = item.Order.OrderNumber,
                UserId = item.Order.UserId,
                UserFullName = item.User.FirstName + " " + item.User.LastName,
                UserEmail = item.User.Email ?? string.Empty,
                OrderDate = item.Order.OrderDate,
                TotalAmount = item.Order.TotalAmount,
                Status = item.Order.Status.ToString()
            })
            .ToListAsync();

        return new PagedResponse<AdminOrderListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ServiceResult<AdminOrderDetailDto>> GetByIdAsync(int id)
    {
        var order = await (
            from orderItem in _dbContext.Orders.AsNoTracking()
            join user in _dbContext.Users.AsNoTracking() on orderItem.UserId equals user.Id
            where orderItem.Id == id
            select new AdminOrderDetailDto
            {
                Id = orderItem.Id,
                OrderNumber = orderItem.OrderNumber,
                UserId = orderItem.UserId,
                UserFullName = user.FirstName + " " + user.LastName,
                UserEmail = user.Email ?? string.Empty,
                OrderDate = orderItem.OrderDate,
                TotalAmount = orderItem.TotalAmount,
                Status = orderItem.Status.ToString(),
                Items = orderItem.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return ServiceResult<AdminOrderDetailDto>.Failure(404, "Sipariş bulunamadı.");
        }

        return ServiceResult<AdminOrderDetailDto>.Success(order);
    }

    public async Task<ServiceResult<AdminOrderDetailDto>> UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request)
    {
        if (!TryParseStatus(request.Status, allowPending: false, out var requestedStatus))
        {
            return ServiceResult<AdminOrderDetailDto>.Failure(400, "Geçersiz sipariş durumu.");
        }

        var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == id);

        if (order is null)
        {
            return ServiceResult<AdminOrderDetailDto>.Failure(404, "Sipariş bulunamadı.");
        }

        if (order.Status == requestedStatus)
        {
            return await GetByIdAsync(id);
        }

        if (order.Status != OrderStatus.Pending)
        {
            return ServiceResult<AdminOrderDetailDto>.Failure(
                400,
                "Onaylanmış veya reddedilmiş siparişin durumu değiştirilemez.");
        }

        order.Status = requestedStatus;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private static bool TryParseStatus(string? value, bool allowPending, out OrderStatus status)
    {
        status = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase: true, out OrderStatus parsedStatus))
        {
            return false;
        }

        if (!Enum.IsDefined(parsedStatus))
        {
            return false;
        }

        if (parsedStatus == OrderStatus.Pending && !allowPending)
        {
            return false;
        }

        status = parsedStatus;
        return true;
    }
}
