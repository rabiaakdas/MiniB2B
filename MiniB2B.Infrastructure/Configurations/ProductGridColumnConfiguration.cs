using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;

namespace MiniB2B.Infrastructure.Configurations;

public class ProductGridColumnConfiguration : IEntityTypeConfiguration<ProductGridColumn>
{
    public void Configure(EntityTypeBuilder<ProductGridColumn> builder)
    {
        builder.ToTable("ProductGridColumns", table =>
        {
            table.HasCheckConstraint("CK_ProductGridColumns_Width_PositiveOrNull", "[Width] IS NULL OR [Width] > 0");
        });

        builder.HasKey(column => column.Id);

        builder.Property(column => column.FieldName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(column => column.HeaderText)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(column => column.DisplayOrder)
            .IsRequired();

        builder.Property(column => column.RenderType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(column => column.Width);

        builder.Property(column => column.Alignment)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(column => column.IsVisibleDesktop)
            .IsRequired();

        builder.Property(column => column.IsVisibleTablet)
            .IsRequired();

        builder.Property(column => column.IsVisibleMobile)
            .IsRequired();

        builder.Property(column => column.IsActive)
            .IsRequired();

        builder.Property(column => column.CreatedAt)
            .IsRequired();

        builder.Property(column => column.UpdatedAt);

        builder.HasIndex(column => new { column.IsActive, column.DisplayOrder });
    }
}
