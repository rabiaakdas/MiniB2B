using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Grid;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class ProductGridService : IProductGridService
{
    private const int MaxPageSize = 100;

    private static readonly HashSet<string> SupportedFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ProductCode",
        "Name",
        "Description",
        "Brand",
        "ManufacturerCode",
        "SpecialCode1",
        "SpecialCode2",
        "ImagePath",
        "StockQuantity",
        "CriticalStockLevel",
        "Price",
        "StockStatus",
        "Quantity",
        "AddToCart"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ProductGridService> _logger;

    public ProductGridService(
        ApplicationDbContext dbContext,
        ILogger<ProductGridService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductGridColumnDto>> GetActiveColumnsAsync()
    {
        var columns = await GetSupportedActiveColumnsAsync();

        return columns.Select(ToDto).ToList();
    }

    public async Task<ProductGridResponse> GetGridAsync(int page, int pageSize, string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var columns = await GetSupportedActiveColumnsAsync();
        var query = ApplySearch(GetVisibleProducts(), search);
        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductGridResponse
        {
            Columns = columns.Select(ToDto).ToList(),
            Items = products.Select(product => ToDynamicRow(product, columns)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private async Task<List<ProductGridColumn>> GetSupportedActiveColumnsAsync()
    {
        var columns = await _dbContext.ProductGridColumns
            .AsNoTracking()
            .Where(column => column.IsActive)
            .OrderBy(column => column.DisplayOrder)
            .ThenBy(column => column.Id)
            .ToListAsync();

        var unsupportedColumns = columns
            .Where(column => !SupportedFieldNames.Contains(column.FieldName))
            .ToList();

        foreach (var unsupportedColumn in unsupportedColumns)
        {
            _logger.LogWarning(
                "Unsupported active product grid field '{FieldName}' ignored. ColumnId: {ColumnId}",
                unsupportedColumn.FieldName,
                unsupportedColumn.Id);
        }

        return columns
            .Where(column => SupportedFieldNames.Contains(column.FieldName))
            .ToList();
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

    private static ProductGridColumnDto ToDto(ProductGridColumn column)
    {
        return new ProductGridColumnDto
        {
            Id = column.Id,
            FieldName = column.FieldName,
            HeaderText = column.HeaderText,
            DisplayOrder = column.DisplayOrder,
            RenderType = column.RenderType.ToString(),
            Width = column.Width,
            Alignment = column.Alignment.ToString(),
            IsVisibleDesktop = column.IsVisibleDesktop,
            IsVisibleTablet = column.IsVisibleTablet,
            IsVisibleMobile = column.IsVisibleMobile
        };
    }

    private static DynamicProductRowDto ToDynamicRow(Product product, IReadOnlyList<ProductGridColumn> columns)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in columns)
        {
            if (IsUiActionField(column.FieldName))
            {
                continue;
            }

            values[column.FieldName] = ResolveValue(product, column.FieldName);
        }

        return new DynamicProductRowDto
        {
            ProductId = product.Id,
            Values = values
        };
    }

    private static object? ResolveValue(Product product, string fieldName)
    {
        return fieldName switch
        {
            "ProductCode" => product.ProductCode,
            "Name" => product.Name,
            "Description" => product.Description,
            "Brand" => product.Brand,
            "ManufacturerCode" => product.ManufacturerCode,
            "SpecialCode1" => product.SpecialCode1,
            "SpecialCode2" => product.SpecialCode2,
            "ImagePath" => product.ImagePath,
            "StockQuantity" => product.StockQuantity,
            "CriticalStockLevel" => product.CriticalStockLevel,
            "Price" => product.Price,
            "StockStatus" => StockStatusHelper.GetStockStatus(product.StockQuantity, product.CriticalStockLevel),
            _ => null
        };
    }

    private static bool IsUiActionField(string fieldName)
    {
        return fieldName.Equals("Quantity", StringComparison.OrdinalIgnoreCase)
            || fieldName.Equals("AddToCart", StringComparison.OrdinalIgnoreCase);
    }
}
