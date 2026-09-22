using Microsoft.EntityFrameworkCore;
using MiniB2B.Domain.Entities;
using MiniB2B.Domain.Enums;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class DevelopmentDataSeedService
{
    private readonly ApplicationDbContext _dbContext;

    public DevelopmentDataSeedService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(bool seedDevelopmentData)
    {
        if (!seedDevelopmentData)
        {
            return;
        }

        await SeedCategoriesAsync();
        await SeedProductGridColumnsAsync();
        await SeedProductsAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        await EnsureCategoryAsync("Ofis Mobilyaları", "Ofis koltuğu, masa ve düzenleyici ürünleri", 1);
        await EnsureCategoryAsync("Bilgisayar Aksesuarları", "Kurumsal bilgisayar çevre birimleri", 2);
        await EnsureCategoryAsync("Kırtasiye", "Ofis sarf ve kırtasiye ürünleri", 3);
        await EnsureCategoryAsync("Depo ve Etiketleme", "Depo, etiketleme ve barkod ekipmanları", 4);
    }

    private async Task SeedProductGridColumnsAsync()
    {
        if (await _dbContext.ProductGridColumns.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        _dbContext.ProductGridColumns.AddRange(
            new ProductGridColumn
            {
                FieldName = "ImagePath",
                HeaderText = "Görsel",
                DisplayOrder = 1,
                RenderType = GridColumnRenderType.Image,
                Width = 80,
                Alignment = GridColumnAlignment.Center,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = false,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "ProductCode",
                HeaderText = "Ürün Kodu",
                DisplayOrder = 2,
                RenderType = GridColumnRenderType.Text,
                Width = 130,
                Alignment = GridColumnAlignment.Left,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = true,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "Name",
                HeaderText = "Ürün Adı",
                DisplayOrder = 3,
                RenderType = GridColumnRenderType.Text,
                Width = 220,
                Alignment = GridColumnAlignment.Left,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = true,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "Brand",
                HeaderText = "Marka",
                DisplayOrder = 4,
                RenderType = GridColumnRenderType.Text,
                Width = 130,
                Alignment = GridColumnAlignment.Left,
                IsVisibleDesktop = true,
                IsVisibleTablet = false,
                IsVisibleMobile = false,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "StockStatus",
                HeaderText = "Stok",
                DisplayOrder = 5,
                RenderType = GridColumnRenderType.StockStatus,
                Width = 100,
                Alignment = GridColumnAlignment.Center,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = true,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "Price",
                HeaderText = "Fiyat",
                DisplayOrder = 6,
                RenderType = GridColumnRenderType.Currency,
                Width = 120,
                Alignment = GridColumnAlignment.Right,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = true,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "Quantity",
                HeaderText = "Miktar",
                DisplayOrder = 7,
                RenderType = GridColumnRenderType.QuantityInput,
                Width = 100,
                Alignment = GridColumnAlignment.Center,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = true,
                IsActive = true,
                CreatedAt = now
            },
            new ProductGridColumn
            {
                FieldName = "AddToCart",
                HeaderText = "Sepet",
                DisplayOrder = 8,
                RenderType = GridColumnRenderType.AddToCart,
                Width = 110,
                Alignment = GridColumnAlignment.Center,
                IsVisibleDesktop = true,
                IsVisibleTablet = true,
                IsVisibleMobile = true,
                IsActive = true,
                CreatedAt = now
            });

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedProductsAsync()
    {
        var categoryIds = await _dbContext.Categories
            .Where(category => category.IsActive)
            .ToDictionaryAsync(category => category.Name, category => category.Id);

        var demoProducts = new[]
        {
            new Product
            {
                ProductCode = "OFS-CHR-001",
                Name = "Ergonomik Ofis Koltuğu",
                Description = "Ayarlanabilir kolçaklı, file sırt destekli ofis koltuğu.",
                Brand = "Nova Office",
                ManufacturerCode = "NO-ERG-100",
                SpecialCode1 = "MOBILYA",
                SpecialCode2 = "PREMIUM",
                StockQuantity = 24,
                CriticalStockLevel = 5,
                Price = 4850.00m,
                CategoryId = categoryIds["Ofis Mobilyaları"]
            },
            new Product
            {
                ProductCode = "ELK-KBD-002",
                Name = "Kablosuz Klavye",
                Description = "Sessiz tuşlu, kompakt kablosuz klavye.",
                Brand = "Teknova",
                ManufacturerCode = "TK-KBD-240",
                SpecialCode1 = "AKSESUAR",
                SpecialCode2 = "STANDART",
                StockQuantity = 42,
                CriticalStockLevel = 8,
                Price = 790.00m,
                CategoryId = categoryIds["Bilgisayar Aksesuarları"]
            },
            new Product
            {
                ProductCode = "ELK-DCK-003",
                Name = "USB-C Dock İstasyonu",
                Description = "HDMI, ethernet ve USB bağlantı destekli çoklayıcı dock.",
                Brand = "Portline",
                ManufacturerCode = "PL-DCK-310",
                SpecialCode1 = "AKSESUAR",
                SpecialCode2 = "B2B",
                StockQuantity = 16,
                CriticalStockLevel = 4,
                Price = 2250.00m,
                CategoryId = categoryIds["Bilgisayar Aksesuarları"]
            },
            new Product
            {
                ProductCode = "ELK-MON-024",
                Name = "24 İnç LED Monitör",
                Description = "Full HD çözünürlüklü, ofis kullanımı için LED monitör.",
                Brand = "ViewPro",
                ManufacturerCode = "VP-LED-24F",
                SpecialCode1 = "EKRAN",
                SpecialCode2 = "OFIS",
                StockQuantity = 12,
                CriticalStockLevel = 3,
                Price = 3890.00m,
                CategoryId = categoryIds["Bilgisayar Aksesuarları"]
            },
            new Product
            {
                ProductCode = "KRT-PPR-004",
                Name = "A4 Fotokopi Kağıdı",
                Description = "80 gr, 500 yapraklı A4 fotokopi kağıdı.",
                Brand = "Paperline",
                ManufacturerCode = "PP-A4-80",
                SpecialCode1 = "SARF",
                SpecialCode2 = "HIZLI",
                StockQuantity = 180,
                CriticalStockLevel = 25,
                Price = 165.00m,
                CategoryId = categoryIds["Kırtasiye"]
            },
            new Product
            {
                ProductCode = "OFS-ORG-005",
                Name = "Masaüstü Evrak Düzenleyici",
                Description = "Üç katlı metal masaüstü evrak düzenleyici.",
                Brand = "DeskPro",
                ManufacturerCode = "DP-ORG-03",
                SpecialCode1 = "OFIS",
                SpecialCode2 = "DUZEN",
                StockQuantity = 31,
                CriticalStockLevel = 6,
                Price = 420.00m,
                CategoryId = categoryIds["Ofis Mobilyaları"]
            },
            new Product
            {
                ProductCode = "DPO-LBL-006",
                Name = "Termal Etiket Rulosu",
                Description = "100x150 mm ölçülerinde termal kargo etiket rulosu.",
                Brand = "LabelMax",
                ManufacturerCode = "LM-100150",
                SpecialCode1 = "ETIKET",
                SpecialCode2 = "DEPO",
                StockQuantity = 75,
                CriticalStockLevel = 15,
                Price = 235.00m,
                CategoryId = categoryIds["Depo ve Etiketleme"]
            },
            new Product
            {
                ProductCode = "DPO-BRC-007",
                Name = "Barkod Okuyucu",
                Description = "USB bağlantılı, 1D/2D destekli barkod okuyucu.",
                Brand = "ScanPoint",
                ManufacturerCode = "SP-BRC-2D",
                SpecialCode1 = "BARKOD",
                SpecialCode2 = "PRO",
                StockQuantity = 9,
                CriticalStockLevel = 3,
                Price = 3150.00m,
                CategoryId = categoryIds["Depo ve Etiketleme"]
            }
        };

        var productCodes = demoProducts.Select(product => product.ProductCode).ToArray();
        var existingCodes = await _dbContext.Products
            .Where(product => productCodes.Contains(product.ProductCode))
            .Select(product => product.ProductCode)
            .ToListAsync();

        var existingCodeSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var product in demoProducts.Where(product => !existingCodeSet.Contains(product.ProductCode)))
        {
            product.IsActive = true;
            product.CreatedAt = now;
            _dbContext.Products.Add(product);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task EnsureCategoryAsync(string name, string description, int displayOrder)
    {
        var existingCategory = await _dbContext.Categories.FirstOrDefaultAsync(category => category.Name == name);

        if (existingCategory is not null)
        {
            existingCategory.Description = description;
            existingCategory.DisplayOrder = displayOrder;
            existingCategory.IsActive = true;
            await _dbContext.SaveChangesAsync();
            return;
        }

        _dbContext.Categories.Add(new Category
        {
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }
}
