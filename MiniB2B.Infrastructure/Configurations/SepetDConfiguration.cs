using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;

namespace MiniB2B.Infrastructure.Configurations;

public class SepetDConfiguration : IEntityTypeConfiguration<SepetD>
{
    public void Configure(EntityTypeBuilder<SepetD> builder)
    {
        builder.ToTable("SepetD", table =>
        {
            table.HasCheckConstraint("CK_SepetD_Quantity_Positive", "[Quantity] > 0");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.SepetRId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.Property(item => item.UpdatedAt);

        // CartItem tarafındaki sepet-ürün indexini SepetD yapısına uyarladım.
        builder.HasIndex(item => new { item.SepetRId, item.ProductId })
            .IsUnique();

        builder.HasOne(item => item.SepetR)
            .WithMany(cart => cart.Items)
            .HasForeignKey(item => item.SepetRId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
