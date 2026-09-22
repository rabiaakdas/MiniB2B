using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Orders;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Entities;
using MiniB2B.Domain.Enums;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class OrderService : IOrderService
{
    private const int MaxPageSize = 100;
    private const string StockChangedMessage =
        "Sepetinizdeki ürünlerden birinin stok durumu değişti. Lütfen sepetinizi kontrol edip tekrar deneyin.";

    private readonly ApplicationDbContext _dbContext;

    public OrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<CreateOrderResponseDto>> CreateOrderAsync(int userId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var cart = await _dbContext.Carts
                .Include(item => item.Items)
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (cart is null || cart.Items.Count == 0)
            {
                await transaction.RollbackAsync();
                return ServiceResult<CreateOrderResponseDto>.Failure(400, "Sepetiniz boş.");
            }

            var cartItems = cart.Items.ToList();

            if (cartItems.Any(item => item.Quantity <= 0))
            {
                await transaction.RollbackAsync();
                return ServiceResult<CreateOrderResponseDto>.Failure(400, "Sepetinizde geçersiz miktarlı ürün bulunmaktadır.");
            }

            var productIds = cartItems
                .Select(item => item.ProductId)
                .Distinct()
                .ToList();

            var products = await _dbContext.Products
                .Include(product => product.Category)
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);

            foreach (var cartItem in cartItems)
            {
                if (!products.TryGetValue(cartItem.ProductId, out var product))
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<CreateOrderResponseDto>.Failure(400, "Sepetinizde artık mevcut olmayan bir ürün bulunmaktadır.");
                }

                if (!product.IsActive || !product.Category.IsActive)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<CreateOrderResponseDto>.Failure(400, $"Ürün {product.Name} artık sipariş verilebilir durumda değildir.");
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<CreateOrderResponseDto>.Failure(
                        400,
                        $"Ürün {product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.StockQuantity}.");
                }
            }

            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            foreach (var cartItem in cartItems)
            {
                var product = products[cartItem.ProductId];
                var unitPrice = product.Price;
                var totalPrice = unitPrice * cartItem.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });

                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            order.TotalAmount = order.Items.Sum(item => item.TotalPrice);
            cart.UpdatedAt = DateTime.UtcNow;

            _dbContext.Orders.Add(order);
            _dbContext.CartItems.RemoveRange(cartItems);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<CreateOrderResponseDto>.Success(
                new CreateOrderResponseDto
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount
                },
                201);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return ServiceResult<CreateOrderResponseDto>.Failure(409, StockChangedMessage);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedResponse<OrderListItemDto>> GetUserOrdersAsync(int userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderListItemDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount
            })
            .ToListAsync();

        return new PagedResponse<OrderListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ServiceResult<OrderDetailDto>> GetUserOrderByIdAsync(int userId, int orderId)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Where(item => item.Id == orderId && item.UserId == userId)
            .Select(item => new OrderDetailDto
            {
                Id = item.Id,
                OrderNumber = item.OrderNumber,
                OrderDate = item.OrderDate,
                Status = item.Status.ToString(),
                TotalAmount = item.TotalAmount,
                Items = item.Items
                    .OrderBy(orderItem => orderItem.Id)
                    .Select(orderItem => new OrderItemDto
                    {
                        ProductId = orderItem.ProductId,
                        ProductCode = orderItem.ProductCode,
                        ProductName = orderItem.ProductName,
                        Quantity = orderItem.Quantity,
                        UnitPrice = orderItem.UnitPrice,
                        TotalPrice = orderItem.TotalPrice
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return ServiceResult<OrderDetailDto>.Failure(404, "Sipariş bulunamadı.");
        }

        return ServiceResult<OrderDetailDto>.Success(order);
    }

    private static string GenerateOrderNumber()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
    }
}
