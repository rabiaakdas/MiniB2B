using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Products;
using MiniB2B.Application.Interfaces;
using MiniB2B.Api.Services;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = Roles.Admin)]
public class AdminProductsController : ControllerBase
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

    private readonly IProductService _productService;
    private readonly IImageStorageService _imageStorageService;

    public AdminProductsController(
        IProductService productService,
        IImageStorageService imageStorageService)
    {
        _productService = productService;
        _imageStorageService = imageStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductListItemDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _productService.GetPagedAsync(page, pageSize, search);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound(new { errors = new[] { "Ürün bulunamadı." } });
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return CreatedAtAction(nameof(GetProduct), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/image")]
    [RequestSizeLimit(MaxImageSizeInBytes)]
    public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile file)
    {
        if (file is null)
        {
            return BadRequest(new { errors = new[] { "Dosya zorunludur." } });
        }

        var existingProduct = await _productService.GetByIdAsync(id);

        if (existingProduct is null)
        {
            return NotFound(new { errors = new[] { "Ürün bulunamadı." } });
        }

        var imageResult = await _imageStorageService.SaveImageAsync(file, "products");

        if (!imageResult.Succeeded
            || string.IsNullOrWhiteSpace(imageResult.RelativePath))
        {
            return BadRequest(new { errors = imageResult.Errors });
        }

        try
        {
            var result = await _productService.UpdateImageAsync(id, imageResult.RelativePath);

            if (!result.Succeeded || result.Data is null)
            {
                _imageStorageService.DeleteIfSafe(imageResult.RelativePath, "products");
                return ToErrorResult(result);
            }

            _imageStorageService.DeleteIfSafe(existingProduct.ImagePath, "products");

            return Ok(result.Data);
        }
        catch
        {
            _imageStorageService.DeleteIfSafe(imageResult.RelativePath, "products");
            throw;
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var existingProduct = await _productService.GetByIdAsync(id);

        if (existingProduct is null)
        {
            return NotFound(new { errors = new[] { "Ürün bulunamadı." } });
        }

        var result = await _productService.DeleteAsync(id);

        if (!result.Succeeded)
        {
            return ToErrorResult(result);
        }

        _imageStorageService.DeleteIfSafe(existingProduct.ImagePath, "products");

        return NoContent();
    }

    private static IActionResult ToErrorResult<T>(ServiceResult<T> result)
    {
        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status404NotFound => new NotFoundObjectResult(errorResponse),
            StatusCodes.Status409Conflict => new ConflictObjectResult(errorResponse),
            _ => new BadRequestObjectResult(errorResponse)
        };
    }

}
