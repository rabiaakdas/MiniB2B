namespace MiniB2B.Domain.Entities;

public class SepetD
{
    public int Id { get; set; }
    // Eski CartItem yapısını sepet kalemlerini tutacak SepetD yapısına uyarladım.
    // SepetRId ile detay kaydını ana sepet kaydına bağladım.
    public int SepetRId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public SepetR SepetR { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
