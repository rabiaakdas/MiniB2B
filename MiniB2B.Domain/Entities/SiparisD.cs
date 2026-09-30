namespace MiniB2B.Domain.Entities;

public class SiparisD
{
    public int Id { get; set; }
    public int SiparisRId { get; set; }
    public int? ProductId { get; set; }
    // OrderItem yapısını R/D modelindeki sipariş detay kaydı olan SiparisD'ye uyarladım.
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public SiparisR SiparisR { get; set; } = null!;
    public Product? Product { get; set; }
}
