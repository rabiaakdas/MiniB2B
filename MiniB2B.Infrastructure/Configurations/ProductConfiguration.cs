using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;

namespace MiniB2B.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            table.HasCheckConstraint("CK_Products_CriticalStockLevel_NonNegative", "[CriticalStockLevel] >= 0");
            table.HasCheckConstraint("CK_Products_Price_NonNegative", "[Price] >= 0");
        });

        builder.HasKey(product => product.Id);

        builder.Property(product => product.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(product => product.Description)
            .HasMaxLength(1000);

        builder.Property(product => product.Brand)
            .HasMaxLength(100);

        builder.Property(product => product.ManufacturerCode)
            .HasMaxLength(100);

        builder.Property(product => product.SpecialCode1)
            .HasMaxLength(100);

        builder.Property(product => product.SpecialCode2)
            .HasMaxLength(100);

        builder.Property(product => product.ImagePath)
            .HasMaxLength(500);

        builder.Property(product => product.StockQuantity)
            .IsRequired();

        builder.Property(product => product.CriticalStockLevel)
            .IsRequired();

        builder.Property(product => product.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(product => product.CategoryId)
            .IsRequired();

        builder.Property(product => product.IsActive)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .IsRequired();

        builder.Property(product => product.UpdatedAt);

        builder.Property(product => product.RowVersion)
            .IsRowVersion();

        builder.HasIndex(product => product.ProductCode)
            .IsUnique();

        builder.HasIndex(product => product.CategoryId);

        builder.HasIndex(product => product.ManufacturerCode);

        builder.HasIndex(product => new { product.IsActive, product.CategoryId });

        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
