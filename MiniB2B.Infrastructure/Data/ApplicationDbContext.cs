using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    // Eski Cart/CartItem/Order/OrderItem DbSet'lerini R/D yapısındaki yeni entity'lere uyarladım.
    public DbSet<SepetR> SepetR => Set<SepetR>();
    public DbSet<SepetD> SepetD => Set<SepetD>();
    public DbSet<SiparisR> SiparisR => Set<SiparisR>();
    public DbSet<SiparisD> SiparisD => Set<SiparisD>();
    public DbSet<ProductGridColumn> ProductGridColumns => Set<ProductGridColumn>();
    public DbSet<Banner> Banners => Set<Banner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
