using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Products;

namespace MiniB2B.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductListItemDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<ProductDetailDto?> GetByIdAsync(int id);
    Task<ServiceResult<ProductDetailDto>> CreateAsync(CreateProductRequest request);
    Task<ServiceResult<ProductDetailDto>> UpdateAsync(int id, UpdateProductRequest request);
    Task<ServiceResult<ProductDetailDto>> UpdateImageAsync(int id, string imagePath);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
