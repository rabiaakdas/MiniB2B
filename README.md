# Mini B2B E-Commerce

## Proje Hakkında

Mini B2B E-Commerce; bayi/kullanıcı odaklı ürün kataloğu, veritabanıyla yönetilen dinamik B2B grid, sepet, sipariş ve admin yönetimi içeren sade bir full-stack uygulamadır.

## Teknolojiler

Backend:
- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT

Veritabanı:
- SQL Server / SQL Server LocalDB

Frontend:
- React
- TypeScript
- Vite
- Axios
- React Router

## Mimari

Proje katmanlı yapı ve Clean Architecture prensipleriyle ayrılmıştır:

- `MiniB2B.Api`: HTTP API, controller, middleware, DI, auth ve static file yayınlama.
- `MiniB2B.Application`: DTO, interface, role sabitleri ve sade response modelleri.
- `MiniB2B.Domain`: Entity ve enum modelleri.
- `MiniB2B.Infrastructure`: EF Core, Identity, DbContext, Fluent API configuration ve servis implementasyonları.
- `MiniB2B.Web`: React + TypeScript frontend.

Dependency yönü Domain'den dışarıya doğru değildir; Domain EF Core, Identity veya ASP.NET Core'a bağımlı değildir.

## Temel Özellikler

- Register/Login ve JWT tabanlı authentication
- Admin/User rolleri
- Admin product management
- Server-side search ve pagination
- DB configurable dynamic B2B grid
- Product catalog ve product detail
- Cart add/update/remove
- Transactional order creation
- OrderItem snapshot
- Product `RowVersion` ile optimistic concurrency
- My Orders
- Account/profile management
- Admin order management ve status update
- Admin user management
- Banner management ve homepage banner

## Dinamik B2B Grid

`ProductGridColumns` tablosu grid davranışını veritabanından kontrol eder:

- `FieldName`: gösterilecek alan veya pseudo-field
- `DisplayOrder`: kolon sırası
- `RenderType`: Text, Image, Currency, StockStatus, QuantityInput, AddToCart
- `Width` ve `Alignment`
- Desktop/tablet/mobile visibility

Frontend kolonları hard-coded basmaz; API'den gelen metadata ile `columns.map(...)` üzerinden header ve cell üretir. Ürün verisi `row.values[column.fieldName]` ile okunur. Böylece kod değişikliği olmadan bayi grid düzeni veritabanından değiştirilebilir.

## Sipariş ve Stok Güvenliği

- Cart stok rezervasyonu yapmaz.
- Sipariş oluşturulmadan hemen önce ürün, kategori, fiyat ve stok tekrar veritabanından okunur.
- Order creation transaction içinde çalışır.
- Başarılı siparişte `Order`, `OrderItems`, stok düşümü ve cart temizleme birlikte commit edilir.
- `OrderItem` içinde `ProductCode`, `ProductName`, `UnitPrice`, `Quantity`, `TotalPrice` snapshot olarak saklanır.
- Product daha sonra değişse bile order history snapshot verisini gösterir.
- `Product.RowVersion` SQL Server rowversion olarak configure edilmiştir ve concurrent stock update senaryolarında optimistic concurrency sağlar.

## Kurulum

Gereksinimler:

- .NET SDK
- Node.js ve npm
- SQL Server veya SQL Server LocalDB

## Backend Ayarları

`MiniB2B.Api/appsettings.json` içinde development için LocalDB connection string bulunur:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MiniB2BDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

JWT signing key repository'ye yazılmaz. Development için User Secrets kullanın:

```powershell
dotnet user-secrets set "Jwt:Key" "YOUR-STRONG-DEVELOPMENT-KEY" --project MiniB2B.Api
```

Development admin seed opsiyoneldir ve yalnızca Development ortamında User Secrets değerleri varsa çalışır:

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "admin@example.com" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "YOUR-STRONG-DEVELOPMENT-PASSWORD" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:FirstName" "Admin" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:LastName" "User" --project MiniB2B.Api
```

Gerçek secret, token veya parola repository'ye eklenmemelidir.

## Development Admin Kurulumu

Repository içinde hazır bir admin parolası bulunmaz. Development ortamında ilk admin hesabı güvenlik nedeniyle User Secrets üzerinden yapılandırılır. Uygulama ilk çalıştığında seed mekanizması bu bilgilerle Admin rolüne sahip hesabı oluşturur. Normal `Kayıt Ol` ekranı yalnız standart `User` hesabı oluşturur; public admin register yoktur.

1. Backend proje klasörünü hedefleyerek User Secrets değerlerini ayarlayın.

`MiniB2B.Api` projesinde `UserSecretsId` zaten tanımlıdır. Gerekirse init komutu şu şekildedir:

```powershell
dotnet user-secrets init --project MiniB2B.Api
```

2. JWT signing key'i ayarlayın. Bu key en az 32 byte uzunluğunda olmalıdır:

```powershell
dotnet user-secrets set "Jwt:Key" "<YOUR_JWT_SECRET_AT_LEAST_32_BYTES>" --project MiniB2B.Api
```

3. Kendi development admin bilgilerinizi ayarlayın:

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "<YOUR_ADMIN_EMAIL>" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "<YOUR_ADMIN_PASSWORD>" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:FirstName" "<YOUR_ADMIN_FIRST_NAME>" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:LastName" "<YOUR_ADMIN_LAST_NAME>" --project MiniB2B.Api
```

Admin parolası mevcut Identity password policy'sine uymalıdır:

- minimum 8 karakter
- en az bir rakam
- en az bir küçük harf
- en az bir büyük harf

4. Veritabanını migration ile oluşturun veya güncelleyin:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project MiniB2B.Infrastructure --startup-project MiniB2B.Api --context ApplicationDbContext
```

5. API'yi Development ortamında çalıştırın:

```powershell
dotnet run --project MiniB2B.Api --urls http://localhost:5051
```

İlk çalışmada:

- `Admin` ve `User` rolleri oluşturulur.
- `DevelopmentAdmin:Email` ve `DevelopmentAdmin:Password` varsa admin hesabı Identity `UserManager` ile oluşturulur.
- Hesap zaten varsa duplicate oluşturulmaz.
- Hesap Admin rolünde değilse Admin rolüne eklenir.

6. Frontend ayarını yapın:

```env
VITE_API_BASE_URL=http://localhost:5051
```

7. Frontend'i çalıştırın:

```powershell
cd MiniB2B.Web
npm install
npm run dev
```

8. Tarayıcıda normal Login ekranından kendi belirlediğiniz admin e-posta/parola ile giriş yapın. JWT içindeki Admin role bilgisiyle frontend `/admin` paneline yönlendirir ve admin endpointleri `[Authorize(Roles = Admin)]` ile korunmaya devam eder.

Sonraki admin hesapları public kayıtla değil, yalnız mevcut yetkili adminin `Hesabım > Yeni Yönetici Ekle` bölümünden oluşturulabilir.

## Veritabanı

Local EF tool restore:

```powershell
dotnet tool restore
```

Migration ile veritabanı oluşturma/güncelleme:

```powershell
dotnet tool run dotnet-ef database update --project MiniB2B.Infrastructure --startup-project MiniB2B.Api --context ApplicationDbContext
```

Alternatif schema scripti:

```text
database/MiniB2B.sql
```

Güncel migration zinciri:

```text
20260918111742_InitialCreate
20260921160840_RemoveCompanyNameFromUsers
20260921162211_RemoveLinkUrlFromBanners
```

`CompanyName` ve `Banner.LinkUrl` aktif model/database alanları değildir. Eski alan adlarının historical migration dosyalarında görünmesi normaldir.

## Backend Çalıştırma

```powershell
dotnet run --project MiniB2B.Api --urls http://localhost:5051
```

Development Swagger:

```text
http://localhost:5051/swagger
```

## Frontend Ayarları

`MiniB2B.Web/.env`:

```env
VITE_API_BASE_URL=http://localhost:5051
```

`.env.example` secret içermez ve aynı key için örnek değer tutar.

## Frontend Çalıştırma

```powershell
cd MiniB2B.Web
npm install
npm run dev
```

Vite development URL:

```text
http://localhost:5173
```

## Roller

- `Admin`
- `User`

Varsayılan parola repository'de bulunmaz. Normal kullanıcı `/register` üzerinden oluşturulabilir. Development admin hesabı gerekiyorsa User Secrets ile seed edilir.

## API Özeti

Auth:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

`GET /api/auth/me`, JWT içindeki kullanıcı id ile current user'ı belirler; ad, soyad, e-posta ve rol bilgilerini güncel Identity kaydından döner.

Account:
- `GET /api/account`
- `PUT /api/account`
- `PUT /api/account/password`

Products:
- `GET /api/products`
- `GET /api/products/{id}`
- `GET /api/products/grid`
- `GET /api/products/grid-columns`

Cart:
- `GET /api/cart`
- `POST /api/cart/items`
- `PUT /api/cart/items/{cartItemId}`
- `DELETE /api/cart/items/{cartItemId}`

Orders:
- `POST /api/orders`
- `GET /api/orders`
- `GET /api/orders/{id}`

Admin:
- `/api/admin/products`
- `/api/admin/orders`
- `/api/admin/users`
- `/api/admin/banners`
- `/api/admin/categories`

Banners:
- `GET /api/banners`

Banner kayıtları `Title`, `Subtitle`, `ImagePath`, `DisplayOrder`, `IsActive`, `StartDate` ve `EndDate` alanlarıyla yönetilir. Public banner endpointi yalnız aktif ve tarih aralığında geçerli bannerları döndürür.

## Güvenlik Notları

- Parolalar ASP.NET Core Identity ile hashlenir.
- PasswordHash DTO veya API response içinde dönmez.
- Kullanıcı kendi hesap bilgilerini `Hesabım` ekranından görüntüleyebilir; ad, soyad, e-posta ve telefon bilgilerini güncelleyebilir.
- Kullanıcı kendi parolasını mevcut parolasını doğrulayarak ASP.NET Core Identity `ChangePasswordAsync` akışı ile değiştirebilir.
- Admin kendi parolasını `Hesabım` ekranından ASP.NET Core Identity `ChangePasswordAsync` akışı ile değiştirebilir.
- Yetkili admin yeni admin hesabı oluşturabilir; normal kullanıcı yönetiminden başka kullanıcıların parolası görüntülenmez veya değiştirilmez.
- Admin endpointleri role authorization ile korunur.
- User ownership gereken kaynaklarda kullanıcı id JWT claim'den alınır.
- Ad ve soyad zorunludur, yalnız whitespace kabul edilmez ve en fazla 100 karakter olabilir.
- E-posta formatı backend tarafında doğrulanır ve duplicate email engellenir.
- Telefon opsiyoneldir; girildiyse `05` ile başlayan tam 11 rakam olmalıdır.
- Upload işlemlerinde dosya boyutu, extension, content-type, random filename ve controlled directory kuralları uygulanır.
- Frontend bu assignment'ın JWT response contract'ı nedeniyle token'ı `localStorage` içinde saklar. Production ortamında XSS riskini azaltmak için HttpOnly + Secure + SameSite cookie tabanlı bir yaklaşım değerlendirilebilir.

## Test / Build

Backend:

```powershell
dotnet restore MiniB2B.slnx
dotnet build MiniB2B.slnx
```

Frontend:

```powershell
cd MiniB2B.Web
npm install
npm run lint
npm run build
```
