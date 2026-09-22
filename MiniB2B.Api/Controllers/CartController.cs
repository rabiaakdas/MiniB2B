using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Extensions;
using MiniB2B.Application.DTOs.Cart;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var cart = await _cartService.GetCartAsync(userId);

        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _cartService.AddItemAsync(userId, request);

        return ToActionResult(result);
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, UpdateCartItemRequest request)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _cartService.UpdateItemAsync(userId, cartItemId, request);

        return ToActionResult(result);
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _cartService.RemoveItemAsync(userId, cartItemId);

        if (!result.Succeeded)
        {
            return ToActionResult(result);
        }

        return NoContent();
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Data);
        }

        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
