using MiniB2B.Domain.Enums;

namespace MiniB2B.Domain.Entities;

public class ProductGridColumn
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public GridColumnRenderType RenderType { get; set; }
    public int? Width { get; set; }
    public GridColumnAlignment Alignment { get; set; } = GridColumnAlignment.Left;
    public bool IsVisibleDesktop { get; set; } = true;
    public bool IsVisibleTablet { get; set; } = true;
    public bool IsVisibleMobile { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
