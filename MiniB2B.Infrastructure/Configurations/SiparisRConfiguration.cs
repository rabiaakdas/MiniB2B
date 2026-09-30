using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Configurations;

public class SiparisRConfiguration : IEntityTypeConfiguration<SiparisR>
{
    public void Configure(EntityTypeBuilder<SiparisR> builder)
    {
        builder.ToTable("SiparisR", table =>
        {
            table.HasCheckConstraint("CK_SiparisR_TotalAmount_NonNegative", "[TotalAmount] >= 0");
        });

        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(order => order.UserId)
            .IsRequired();

        builder.Property(order => order.SepetId)
            .IsRequired(false);

        builder.Property(order => order.OrderDate)
            .IsRequired();

        builder.Property(order => order.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(order => order.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(order => order.OrderNumber)
            .IsUnique();

        builder.HasIndex(order => new { order.UserId, order.OrderDate });

        builder.HasIndex(order => order.Status);

        builder.HasIndex(order => order.SepetId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // SiparisR ile kaynak SepetR arasındaki bağlantıyı SepetId üzerinden kurdum.
        builder.HasOne(order => order.Sepet)
            .WithMany()
            .HasForeignKey(order => order.SepetId)
            .OnDelete(DeleteBehavior.Restrict);

        // SiparisR ile SiparisD arasındaki ana kayıt/detay ilişkisini yeni R/D isimleriyle kurdum.
        builder.HasMany(order => order.Items)
            .WithOne(item => item.SiparisR)
            .HasForeignKey(item => item.SiparisRId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
