using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Categories;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = Roles.Admin)]
public class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryLookupService _categoryLookupService;

    public AdminCategoriesController(ICategoryLookupService categoryLookupService)
    {
        _categoryLookupService = categoryLookupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryLookupDto>>> GetCategories()
    {
        var categories = await _categoryLookupService.GetActiveAsync();

        return Ok(categories);
    }
}
