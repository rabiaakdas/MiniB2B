using MiniB2B.Application.DTOs.Cart;
using MiniB2B.Application.DTOs.Common;

namespace MiniB2B.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<ServiceResult<CartDto>> AddItemAsync(int userId, AddCartItemRequest request);
    Task<ServiceResult<CartDto>> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task<ServiceResult<CartDto>> RemoveItemAsync(int userId, int cartItemId);
}
