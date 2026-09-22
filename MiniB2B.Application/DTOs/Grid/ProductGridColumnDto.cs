namespace MiniB2B.Application.DTOs.Grid;

public class ProductGridColumnDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string RenderType { get; set; } = string.Empty;
    public int? Width { get; set; }
    public string Alignment { get; set; } = string.Empty;
    public bool IsVisibleDesktop { get; set; }
    public bool IsVisibleTablet { get; set; }
    public bool IsVisibleMobile { get; set; }
}
