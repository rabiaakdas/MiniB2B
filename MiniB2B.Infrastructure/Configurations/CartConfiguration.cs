using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.UserId)
            .IsRequired();

        builder.Property(cart => cart.CreatedAt)
            .IsRequired();

        builder.Property(cart => cart.UpdatedAt);

        builder.HasIndex(cart => cart.UserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
