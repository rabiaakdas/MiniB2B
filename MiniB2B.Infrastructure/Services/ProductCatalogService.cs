using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Catalog;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class ProductCatalogService : IProductCatalogService
{
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;

    public ProductCatalogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<CatalogProductDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = ApplySearch(GetVisibleProducts(), search);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new CatalogProductDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Brand = product.Brand,
                ManufacturerCode = product.ManufacturerCode,
                ImagePath = product.ImagePath,
                StockQuantity = product.StockQuantity,
                CriticalStockLevel = product.CriticalStockLevel,
                StockStatus = product.StockQuantity > product.CriticalStockLevel
                    ? "Available"
                    : product.StockQuantity > 0
                        ? "Critical"
                        : "OutOfStock",
                Price = product.Price
            })
            .ToListAsync();

        return new PagedResponse<CatalogProductDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<CatalogProductDetailDto?> GetByIdAsync(int id)
    {
        return await GetVisibleProducts()
            .Where(product => product.Id == id)
            .Select(product => new CatalogProductDetailDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                ManufacturerCode = product.ManufacturerCode,
                SpecialCode1 = product.SpecialCode1,
                SpecialCode2 = product.SpecialCode2,
                ImagePath = product.ImagePath,
                StockQuantity = product.StockQuantity,
                CriticalStockLevel = product.CriticalStockLevel,
                StockStatus = product.StockQuantity > product.CriticalStockLevel
                    ? "Available"
                    : product.StockQuantity > 0
                        ? "Critical"
                        : "OutOfStock",
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            })
            .FirstOrDefaultAsync();
    }

    private IQueryable<Product> GetVisibleProducts()
    {
        return _dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.Category.IsActive);
    }

    private static IQueryable<Product> ApplySearch(IQueryable<Product> query, string? search)
    {
        var trimmedSearch = search?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedSearch))
        {
            return query;
        }

        return query.Where(product =>
            product.ProductCode.Contains(trimmedSearch)
            || product.Name.Contains(trimmedSearch)
            || (product.Description != null && product.Description.Contains(trimmedSearch))
            || (product.Brand != null && product.Brand.Contains(trimmedSearch))
            || (product.ManufacturerCode != null && product.ManufacturerCode.Contains(trimmedSearch))
            || (product.SpecialCode1 != null && product.SpecialCode1.Contains(trimmedSearch))
            || (product.SpecialCode2 != null && product.SpecialCode2.Contains(trimmedSearch)));
    }
}
