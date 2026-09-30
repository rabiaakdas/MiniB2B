using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Configurations;

public class SepetRConfiguration : IEntityTypeConfiguration<SepetR>
{
    public void Configure(EntityTypeBuilder<SepetR> builder)
    {
        builder.ToTable("SepetR", table =>
        {
            table.HasCheckConstraint("CK_SepetR_TotalAmount_NonNegative", "[TotalAmount] >= 0");
        });

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.UserId)
            .IsRequired();

        builder.Property(cart => cart.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(cart => cart.CreatedAt)
            .IsRequired();

        builder.Property(cart => cart.UpdatedAt);

        builder.HasIndex(cart => cart.UserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<SepetR>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // SepetR ile SepetD arasındaki ana kayıt/detay ilişkisini yeni R/D isimleriyle kurdum.
        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.SepetR)
            .HasForeignKey(item => item.SepetRId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
