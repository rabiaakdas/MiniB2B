namespace MiniB2B.Application.DTOs.Grid;

public class ProductGridResponse
{
    public IReadOnlyList<ProductGridColumnDto> Columns { get; set; } = [];
    public IReadOnlyList<DynamicProductRowDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
