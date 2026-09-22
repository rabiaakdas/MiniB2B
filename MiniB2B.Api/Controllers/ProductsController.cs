using Microsoft.AspNetCore.Mvc;
using MiniB2B.Application.DTOs.Catalog;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductCatalogService _productCatalogService;
    private readonly IProductGridService _productGridService;

    public ProductsController(
        IProductCatalogService productCatalogService,
        IProductGridService productGridService)
    {
        _productCatalogService = productCatalogService;
        _productGridService = productGridService;
    }

    [HttpGet("grid-columns")]
    public async Task<IActionResult> GetGridColumns()
    {
        var columns = await _productGridService.GetActiveColumnsAsync();

        return Ok(columns);
    }

    [HttpGet("grid")]
    public async Task<IActionResult> GetGrid(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var grid = await _productGridService.GetGridAsync(page, pageSize, search);

        return Ok(grid);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CatalogProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var products = await _productCatalogService.GetPagedAsync(page, pageSize, search);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogProductDetailDto>> GetProduct(int id)
    {
        var product = await _productCatalogService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound(new { errors = new[] { "Ürün bulunamadı." } });
        }

        return Ok(product);
    }
}
