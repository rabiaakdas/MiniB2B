using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Domain.Entities;

namespace MiniB2B.Infrastructure.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners", table =>
        {
            table.HasCheckConstraint(
                "CK_Banners_DateRange",
                "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");
        });

        builder.HasKey(banner => banner.Id);

        builder.Property(banner => banner.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(banner => banner.Subtitle)
            .HasMaxLength(300);

        builder.Property(banner => banner.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(banner => banner.DisplayOrder)
            .IsRequired();

        builder.Property(banner => banner.IsActive)
            .IsRequired();

        builder.Property(banner => banner.StartDate);

        builder.Property(banner => banner.EndDate);

        builder.HasIndex(banner => new { banner.IsActive, banner.DisplayOrder });
    }
}
