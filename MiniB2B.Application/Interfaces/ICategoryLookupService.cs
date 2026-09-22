using MiniB2B.Application.DTOs.Categories;

namespace MiniB2B.Application.Interfaces;

public interface ICategoryLookupService
{
    Task<IReadOnlyList<CategoryLookupDto>> GetActiveAsync();
}
