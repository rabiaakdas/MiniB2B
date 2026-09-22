using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Categories;
using MiniB2B.Application.Interfaces;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class CategoryLookupService : ICategoryLookupService
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryLookupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryLookupDto>> GetActiveAsync()
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryLookupDto
            {
                Id = category.Id,
                Name = category.Name
            })
            .ToListAsync();
    }
}
