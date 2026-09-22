using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Extensions;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = Roles.Admin)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _adminUserService.GetPagedAsync(page, pageSize, search);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _adminUserService.GetByIdAsync(id);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        if (!User.TryGetUserId(out var currentAdminUserId))
        {
            return Unauthorized();
        }

        var result = await _adminUserService.UpdateAsync(id, currentAdminUserId, request);

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
