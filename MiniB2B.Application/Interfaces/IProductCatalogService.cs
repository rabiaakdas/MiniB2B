using MiniB2B.Application.DTOs.Catalog;
using MiniB2B.Application.DTOs.Common;

namespace MiniB2B.Application.Interfaces;

public interface IProductCatalogService
{
    Task<PagedResponse<CatalogProductDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<CatalogProductDetailDto?> GetByIdAsync(int id);
}
