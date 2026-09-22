using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Extensions;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Orders;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _orderService.GetUserOrdersAsync(userId, page, pageSize);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _orderService.GetUserOrderByIdAsync(userId, id);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _orderService.CreateOrderAsync(userId);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    private IActionResult ToErrorResult<T>(ServiceResult<T> result)
    {
        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(errorResponse),
            StatusCodes.Status409Conflict => Conflict(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
