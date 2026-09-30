namespace MiniB2B.Domain.Entities;

public class SepetR
{
    public int Id { get; set; }
    public int UserId { get; set; }
    // Cart yapısını R/D modelindeki ana sepet kaydı olan SepetR'ye uyarladım.
    // Sepet toplamını ana kayıtta tutmak için TotalAmount alanını ekledim.
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Sepet kalemlerini SepetD detay kayıtları olarak bağladım.
    public ICollection<SepetD> Items { get; set; } = new List<SepetD>();
}
