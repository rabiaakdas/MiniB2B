using MiniB2B.Domain.Enums;

namespace MiniB2B.Domain.Entities;

public class SiparisR
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    // Eski Order yapısını sipariş ana kaydı olan SiparisR'ye uyarladım.
    // Yeni bir sepet id'si üretmedim; siparişin kaynak SepetR kaydını takip etmek için bu bağlantıyı ekledim.
    public int? SepetId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public SepetR? Sepet { get; set; }
    public ICollection<SiparisD> Items { get; set; } = new List<SiparisD>();
}
