using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;

namespace MiniB2B.Infrastructure.Configurations;

public class SiparisDConfiguration : IEntityTypeConfiguration<SiparisD>
{
    public void Configure(EntityTypeBuilder<SiparisD> builder)
    {
        builder.ToTable("SiparisD", table =>
        {
            table.HasCheckConstraint("CK_SiparisD_Quantity_Positive", "[Quantity] > 0");
            table.HasCheckConstraint("CK_SiparisD_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            table.HasCheckConstraint("CK_SiparisD_TotalPrice_NonNegative", "[TotalPrice] >= 0");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.SiparisRId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired(false);

        builder.Property(item => item.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(item => item.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(item => item.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // OrderItem ilişki yapılandırmasını SiparisD adına göre uyarladım.
        builder.HasOne(item => item.SiparisR)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.SiparisRId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
