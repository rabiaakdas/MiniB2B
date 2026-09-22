# Mini B2B E-Commerce

## Proje Hakkında

Mini B2B E-Commerce; bayi/kullanici odakli urun katalogu, veritabaniyla yonetilen dinamik B2B grid, sepet, siparis ve admin yonetimi iceren sade bir full-stack uygulamadir.

## Teknolojiler

Backend:
- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT

Database:
- SQL Server / SQL Server LocalDB

Frontend:
- React
- TypeScript
- Vite
- Axios
- React Router

## Mimari

Proje katmanli yapi ve Clean Architecture prensipleriyle ayrilmistir:

- `MiniB2B.Api`: HTTP API, controller, middleware, DI, auth ve static file yayinlama.
- `MiniB2B.Application`: DTO, interface, role sabitleri ve sade response modelleri.
- `MiniB2B.Domain`: Entity ve enum modelleri.
- `MiniB2B.Infrastructure`: EF Core, Identity, DbContext, Fluent API configuration ve servis implementasyonlari.
- `MiniB2B.Web`: React + TypeScript frontend.

Dependency yonu Domain'den disariya dogru degildir; Domain EF Core, Identity veya ASP.NET Core'a bagimli degildir.

## Temel Ozellikler

- Register/Login ve JWT tabanli authentication
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

`ProductGridColumns` tablosu grid davranisini veritabanindan kontrol eder:

- `FieldName`: gosterilecek alan veya pseudo-field
- `DisplayOrder`: kolon sirasi
- `RenderType`: Text, Image, Currency, StockStatus, QuantityInput, AddToCart
- `Width` ve `Alignment`
- Desktop/tablet/mobile visibility

Frontend kolonlari hard-coded basmaz; API'den gelen metadata ile `columns.map(...)` uzerinden header ve cell uretir. Urun verisi `row.values[column.fieldName]` ile okunur. Boylece kod degisikligi olmadan bayi grid duzeni veritabanindan degistirilebilir.

## Siparis ve Stok Guvenligi

- Cart stok rezervasyonu yapmaz.
- Siparis olusturulmadan hemen once urun, kategori, fiyat ve stok tekrar veritabanindan okunur.
- Order creation transaction icinde calisir.
- Basarili sipariste `Order`, `OrderItems`, stok dusumu ve cart temizleme birlikte commit edilir.
- `OrderItem` icinde `ProductCode`, `ProductName`, `UnitPrice`, `Quantity`, `TotalPrice` snapshot olarak saklanir.
- Product daha sonra degisse bile order history snapshot verisini gosterir.
- `Product.RowVersion` SQL Server rowversion olarak configure edilmistir ve concurrent stock update senaryolarinda optimistic concurrency saglar.

## Kurulum

Gereksinimler:

- .NET SDK
- Node.js ve npm
- SQL Server veya SQL Server LocalDB

## Backend Ayarlari

`MiniB2B.Api/appsettings.json` icinde development icin LocalDB connection string bulunur:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MiniB2BDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

JWT signing key repository'ye yazilmaz. Development icin User Secrets kullanin:

```powershell
dotnet user-secrets set "Jwt:Key" "YOUR-STRONG-DEVELOPMENT-KEY" --project MiniB2B.Api
```

Development admin seed opsiyoneldir ve yalnizca Development ortaminda User Secrets degerleri varsa calisir:

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "admin@example.com" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "YOUR-STRONG-DEVELOPMENT-PASSWORD" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:FirstName" "Admin" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:LastName" "User" --project MiniB2B.Api
```

Gercek secret, token veya parola repository'ye eklenmemelidir.

## Development Admin Kurulumu

Repository icinde hazir bir admin parolasi bulunmaz. Development ortaminda ilk admin hesabi guvenlik nedeniyle User Secrets uzerinden yapilandirilir. Uygulama ilk calistiginda seed mekanizmasi bu bilgilerle Admin rolune sahip hesabi olusturur. Normal `Kayit Ol` ekrani yalniz standart `User` hesabi olusturur; public admin register yoktur.

1. Backend proje klasorunu hedefleyerek User Secrets degerlerini ayarlayin.

`MiniB2B.Api` projesinde `UserSecretsId` zaten tanimlidir. Gerekirse init komutu su sekildedir:

```powershell
dotnet user-secrets init --project MiniB2B.Api
```

2. JWT signing key'i ayarlayin. Bu key en az 32 byte uzunlugunda olmalidir:

```powershell
dotnet user-secrets set "Jwt:Key" "<YOUR_JWT_SECRET_AT_LEAST_32_BYTES>" --project MiniB2B.Api
```

3. Kendi development admin bilgilerinizi ayarlayin:

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "<YOUR_ADMIN_EMAIL>" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "<YOUR_ADMIN_PASSWORD>" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:FirstName" "<YOUR_ADMIN_FIRST_NAME>" --project MiniB2B.Api
dotnet user-secrets set "DevelopmentAdmin:LastName" "<YOUR_ADMIN_LAST_NAME>" --project MiniB2B.Api
```

Admin parolasi mevcut Identity password policy'sine uymalidir:

- minimum 8 karakter
- en az bir rakam
- en az bir kucuk harf
- en az bir buyuk harf

4. Database'i migration ile olusturun veya guncelleyin:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project MiniB2B.Infrastructure --startup-project MiniB2B.Api --context ApplicationDbContext
```

5. API'yi Development ortaminda calistirin:

```powershell
dotnet run --project MiniB2B.Api --urls http://localhost:5051
```

Ilk calismada:

- `Admin` ve `User` rolleri olusturulur.
- `DevelopmentAdmin:Email` ve `DevelopmentAdmin:Password` varsa admin hesabi Identity `UserManager` ile olusturulur.
- Hesap zaten varsa duplicate olusturulmaz.
- Hesap Admin rolunde degilse Admin rolune eklenir.

6. Frontend ayarini yapin:

```env
VITE_API_BASE_URL=http://localhost:5051
```

7. Frontend'i calistirin:

```powershell
cd MiniB2B.Web
npm install
npm run dev
```

8. Tarayicida normal Login ekranindan kendi belirlediginiz admin e-posta/parola ile giris yapin. JWT icindeki Admin role bilgisiyle frontend `/admin` paneline yonlendirir ve admin endpointleri `[Authorize(Roles = Admin)]` ile korunmaya devam eder.

Sonraki admin hesaplari public kayitla degil, yalniz mevcut yetkili adminin `Hesabim > Yeni Yonetici Ekle` bolumunden olusturulabilir.

## Database

Local EF tool restore:

```powershell
dotnet tool restore
```

Migration ile database olusturma/guncelleme:

```powershell
dotnet tool run dotnet-ef database update --project MiniB2B.Infrastructure --startup-project MiniB2B.Api --context ApplicationDbContext
```

Alternatif schema scripti:

```text
database/MiniB2B.sql
```

Guncel migration zinciri:

```text
20260918111742_InitialCreate
20260921160840_RemoveCompanyNameFromUsers
20260921162211_RemoveLinkUrlFromBanners
```

`CompanyName` ve `Banner.LinkUrl` aktif model/database alanlari degildir. Eski alan adlarinin historical migration dosyalarinda gorunmesi normaldir.

## Backend Calistirma

```powershell
dotnet run --project MiniB2B.Api --urls http://localhost:5051
```

Development Swagger:

```text
http://localhost:5051/swagger
```

## Frontend Ayarlari

`MiniB2B.Web/.env`:

```env
VITE_API_BASE_URL=http://localhost:5051
```

`.env.example` secret icermez ve ayni key icin ornek deger tutar.

## Frontend Calistirma

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

Varsayilan parola repository'de bulunmaz. Normal kullanici `/register` uzerinden olusturulabilir. Development admin hesabi gerekiyorsa User Secrets ile seed edilir.

## API Ozeti

Auth:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

`GET /api/auth/me`, JWT icindeki kullanici id ile current user'i belirler; ad, soyad, e-posta ve rol bilgilerini guncel Identity kaydindan doner.

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

Banner kayitlari `Title`, `Subtitle`, `ImagePath`, `DisplayOrder`, `IsActive`, `StartDate` ve `EndDate` alanlariyla yonetilir. Public banner endpointi yalniz aktif ve tarih araliginda gecerli bannerlari dondurur.

## Guvenlik Notlari

- Parolalar ASP.NET Core Identity ile hashlenir.
- PasswordHash DTO veya API response icinde donmez.
- Kullanici kendi hesap bilgilerini `Hesabim` ekranindan goruntuleyebilir; ad, soyad, e-posta ve telefon bilgilerini guncelleyebilir.
- Kullanici kendi parolasini mevcut parolasini dogrulayarak ASP.NET Core Identity `ChangePasswordAsync` akisi ile degistirebilir.
- Admin kendi parolasini `Hesabim` ekranindan ASP.NET Core Identity `ChangePasswordAsync` akisi ile degistirebilir.
- Yetkili admin yeni admin hesabi olusturabilir; normal kullanici yonetiminden baska kullanicilarin parolasi goruntulenmez veya degistirilmez.
- Admin endpointleri role authorization ile korunur.
- User ownership gereken kaynaklarda kullanici id JWT claim'den alinir.
- Ad ve soyad zorunludur, yalniz whitespace kabul edilmez ve en fazla 100 karakter olabilir.
- E-posta formati backend tarafinda dogrulanir ve duplicate email engellenir.
- Telefon opsiyoneldir; girildiyse `05` ile baslayan tam 11 rakam olmalidir.
- Upload islemlerinde dosya boyutu, extension, content-type, random filename ve controlled directory kurallari uygulanir.
- Frontend bu assignment'in JWT response contract'i nedeniyle token'i `localStorage` icinde saklar. Production ortaminda XSS riskini azaltmak icin HttpOnly + Secure + SameSite cookie tabanli bir yaklasim degerlendirilebilir.

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

## Veritabani Notlari

`OrderItem.ProductId` nullable'dir. Urun fiziksel olarak silinse veya daha sonra degisse bile siparis gecmisi `OrderItem` snapshot alanlariyla gosterilir. Normal uygulama akisinda urun silmek yerine `IsActive=false` kullanilir.
