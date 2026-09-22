namespace MiniB2B.Application.DTOs.Grid;

public class DynamicProductRowDto
{
    public int ProductId { get; set; }
    public Dictionary<string, object?> Values { get; set; } = [];
}
