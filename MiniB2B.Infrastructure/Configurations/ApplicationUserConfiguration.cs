using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();
    }
}
