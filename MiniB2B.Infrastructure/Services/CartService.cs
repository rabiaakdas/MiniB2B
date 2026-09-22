using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Cart;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _dbContext;

    public CartService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var items = await GetCartItemsQuery(userId).ToListAsync();

        return ToCartDto(items);
    }

    public async Task<ServiceResult<CartDto>> AddItemAsync(int userId, AddCartItemRequest request)
    {
        if (request.ProductId <= 0 || request.Quantity <= 0)
        {
            return ServiceResult<CartDto>.Failure(400, "ProductId ve Quantity pozitif olmalıdır.");
        }

        var product = await _dbContext.Products
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == request.ProductId);

        var validationError = ValidateProductForCart(product);

        if (validationError is not null)
        {
            return ServiceResult<CartDto>.Failure(400, validationError);
        }

        var cart = await _dbContext.Carts
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Carts.Add(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == request.ProductId);
        var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (requestedTotalQuantity > product!.StockQuantity)
        {
            return ServiceResult<CartDto>.Failure(
                400,
                $"Ürün {product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.StockQuantity}.");
        }

        if (existingItem is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingItem.Quantity = requestedTotalQuantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ServiceResult<CartDto>.Success(await GetCartAsync(userId));
    }

    public async Task<ServiceResult<CartDto>> UpdateItemAsync(
        int userId,
        int cartItemId,
        UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            return ServiceResult<CartDto>.Failure(400, "Quantity pozitif olmalıdır.");
        }

        var cartItem = await _dbContext.CartItems
            .Include(item => item.Cart)
            .Include(item => item.Product)
            .ThenInclude(product => product.Category)
            .FirstOrDefaultAsync(item => item.Id == cartItemId && item.Cart.UserId == userId);

        if (cartItem is null)
        {
            return ServiceResult<CartDto>.Failure(404, "Sepet ürünü bulunamadı.");
        }

        var validationError = ValidateProductForCart(cartItem.Product);

        if (validationError is not null)
        {
            return ServiceResult<CartDto>.Failure(400, validationError);
        }

        if (request.Quantity > cartItem.Product.StockQuantity)
        {
            return ServiceResult<CartDto>.Failure(
                400,
                $"Ürün {cartItem.Product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {cartItem.Product.StockQuantity}.");
        }

        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        cartItem.Cart.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ServiceResult<CartDto>.Success(await GetCartAsync(userId));
    }

    public async Task<ServiceResult<CartDto>> RemoveItemAsync(int userId, int cartItemId)
    {
        var cartItem = await _dbContext.CartItems
            .Include(item => item.Cart)
            .FirstOrDefaultAsync(item => item.Id == cartItemId && item.Cart.UserId == userId);

        if (cartItem is null)
        {
            return ServiceResult<CartDto>.Failure(404, "Sepet ürünü bulunamadı.");
        }

        cartItem.Cart.UpdatedAt = DateTime.UtcNow;
        _dbContext.CartItems.Remove(cartItem);

        await _dbContext.SaveChangesAsync();

        return ServiceResult<CartDto>.Success(await GetCartAsync(userId));
    }

    private IQueryable<CartItemDto> GetCartItemsQuery(int userId)
    {
        return _dbContext.CartItems
            .AsNoTracking()
            .Where(item => item.Cart.UserId == userId)
            .OrderBy(item => item.Id)
            .Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductCode = item.Product.ProductCode,
                ProductName = item.Product.Name,
                ImagePath = item.Product.ImagePath,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Product.Price * item.Quantity,
                StockQuantity = item.Product.StockQuantity,
                StockStatus = item.Product.StockQuantity > item.Product.CriticalStockLevel
                    ? StockStatusHelper.Available
                    : item.Product.StockQuantity > 0
                        ? StockStatusHelper.Critical
                        : StockStatusHelper.OutOfStock,
                IsAvailable = item.Product.IsActive
                    && item.Product.Category.IsActive
                    && item.Product.StockQuantity > 0
            });
    }

    private static CartDto ToCartDto(IReadOnlyList<CartItemDto> items)
    {
        return new CartDto
        {
            Items = items,
            TotalAmount = items.Sum(item => item.LineTotal),
            TotalItemCount = items.Sum(item => item.Quantity)
        };
    }

    private static string? ValidateProductForCart(Product? product)
    {
        if (product is null)
        {
            return "Ürün bulunamadı.";
        }

        if (!product.IsActive || !product.Category.IsActive)
        {
            return "Ürün sepete eklenemez.";
        }

        if (product.StockQuantity <= 0)
        {
            return "Ürün stokta bulunmamaktadır.";
        }

        return null;
    }
}
