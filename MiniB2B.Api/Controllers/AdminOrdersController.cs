using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Orders;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = Roles.Admin)]
public class AdminOrdersController : ControllerBase
{
    private readonly IAdminOrderService _adminOrderService;

    public AdminOrdersController(IAdminOrderService adminOrderService)
    {
        _adminOrderService = adminOrderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var result = await _adminOrderService.GetPagedAsync(page, pageSize, search, status);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var result = await _adminOrderService.GetByIdAsync(id);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Data);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var result = await _adminOrderService.UpdateStatusAsync(id, request);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Data);
    }

    private IActionResult ToErrorResult<T>(ServiceResult<T> result)
    {
        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
