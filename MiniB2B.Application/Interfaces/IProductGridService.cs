using MiniB2B.Application.DTOs.Grid;

namespace MiniB2B.Application.Interfaces;

public interface IProductGridService
{
    Task<IReadOnlyList<ProductGridColumnDto>> GetActiveColumnsAsync();
    Task<ProductGridResponse> GetGridAsync(int page, int pageSize, string? search);
}
