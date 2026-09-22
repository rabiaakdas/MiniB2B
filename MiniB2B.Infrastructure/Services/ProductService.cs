using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Products;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class ProductService : IProductService
{
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;

    public ProductService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<ProductListItemDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = ApplySearch(_dbContext.Products.AsNoTracking(), search);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductListItemDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                ManufacturerCode = product.ManufacturerCode,
                SpecialCode1 = product.SpecialCode1,
                SpecialCode2 = product.SpecialCode2,
                StockQuantity = product.StockQuantity,
                CriticalStockLevel = product.CriticalStockLevel,
                Price = product.Price,
                ImagePath = product.ImagePath,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive
            })
            .ToListAsync();

        return new PagedResponse<ProductListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Select(product => new ProductDetailDto
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
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<ProductDetailDto>> CreateAsync(CreateProductRequest request)
    {
        var validationErrors = await ValidateProductAsync(request.ProductCode, request.Name, request.CategoryId);

        if (validationErrors.Count > 0)
        {
            return ServiceResult<ProductDetailDto>.Failure(400, validationErrors.ToArray());
        }

        var product = new Product
        {
            ProductCode = request.ProductCode.Trim(),
            Name = request.Name.Trim(),
            Description = TrimToNull(request.Description),
            Brand = TrimToNull(request.Brand),
            ManufacturerCode = TrimToNull(request.ManufacturerCode),
            SpecialCode1 = TrimToNull(request.SpecialCode1),
            SpecialCode2 = TrimToNull(request.SpecialCode2),
            StockQuantity = request.StockQuantity,
            CriticalStockLevel = request.CriticalStockLevel,
            Price = request.Price,
            CategoryId = request.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var detail = await GetByIdAsync(product.Id)
            ?? throw new InvalidOperationException("Created product could not be loaded.");

        return ServiceResult<ProductDetailDto>.Success(detail, 201);
    }

    public async Task<ServiceResult<ProductDetailDto>> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return ServiceResult<ProductDetailDto>.Failure(404, "Ürün bulunamadı.");
        }

        var validationErrors = await ValidateProductAsync(request.ProductCode, request.Name, request.CategoryId, id);

        if (validationErrors.Count > 0)
        {
            return ServiceResult<ProductDetailDto>.Failure(400, validationErrors.ToArray());
        }

        product.ProductCode = request.ProductCode.Trim();
        product.Name = request.Name.Trim();
        product.Description = TrimToNull(request.Description);
        product.Brand = TrimToNull(request.Brand);
        product.ManufacturerCode = TrimToNull(request.ManufacturerCode);
        product.SpecialCode1 = TrimToNull(request.SpecialCode1);
        product.SpecialCode2 = TrimToNull(request.SpecialCode2);
        product.StockQuantity = request.StockQuantity;
        product.CriticalStockLevel = request.CriticalStockLevel;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var detail = await GetByIdAsync(product.Id)
            ?? throw new InvalidOperationException("Updated product could not be loaded.");

        return ServiceResult<ProductDetailDto>.Success(detail);
    }

    public async Task<ServiceResult<ProductDetailDto>> UpdateImageAsync(int id, string imagePath)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return ServiceResult<ProductDetailDto>.Failure(404, "Ürün bulunamadı.");
        }

        product.ImagePath = imagePath;
        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var detail = await GetByIdAsync(product.Id)
            ?? throw new InvalidOperationException("Updated product could not be loaded.");

        return ServiceResult<ProductDetailDto>.Success(detail);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return ServiceResult<bool>.Failure(404, "Ürün bulunamadı.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var cartItems = await _dbContext.CartItems
            .Where(item => item.ProductId == id)
            .ToListAsync();

        if (cartItems.Count > 0)
        {
            _dbContext.CartItems.RemoveRange(cartItems);
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return ServiceResult<bool>.Success(true);
    }

    private async Task<List<string>> ValidateProductAsync(
        string productCode,
        string name,
        int categoryId,
        int? currentProductId = null)
    {
        var errors = new List<string>();
        var trimmedProductCode = productCode.Trim();
        var trimmedName = name.Trim();

        if (string.IsNullOrWhiteSpace(trimmedProductCode))
        {
            errors.Add("Ürün kodu zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            errors.Add("Ürün adı zorunludur.");
        }

        var productCodeExists = await _dbContext.Products.AnyAsync(product =>
            product.ProductCode == trimmedProductCode
            && (!currentProductId.HasValue || product.Id != currentProductId.Value));

        if (productCodeExists)
        {
            errors.Add("Bu ürün kodu zaten kullanılıyor.");
        }

        var categoryExists = await _dbContext.Categories.AnyAsync(category =>
            category.Id == categoryId && category.IsActive);

        if (!categoryExists)
        {
            errors.Add("Geçerli ve aktif bir kategori seçilmelidir.");
        }

        return errors;
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

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
