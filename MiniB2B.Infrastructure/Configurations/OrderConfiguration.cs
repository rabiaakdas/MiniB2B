using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_TotalAmount_NonNegative", "[TotalAmount] >= 0");
        });

        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(order => order.UserId)
            .IsRequired();

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

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
