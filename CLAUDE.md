# ViaCekek — Marina Çekek Sahası Giriş/Çıkış Takip Sistemi

## Proje Özeti

Marina çekek sahasına giren ve çıkan kişi ve araçları takip eden bir web
uygulaması. Tablet ve bilgisayar tarayıcısında çalışır. Sahada bir tablet
**sürekli açık kalarak** giriş/çıkış kayıt ekranı olarak kullanılır; bu
ekranın kesintisiz ve sorunsuz çalışması projenin en kritik gereksinimidir.

Ölçek: günde ortalama ~300 kişi giriş/çıkış işlemi (düşük-orta trafik,
yüksek eşzamanlılık beklenmiyor).

Veritabanı şeması Entity Framework Core Code-First migration'ları ile
Claude tarafından oluşturulur: her yeni/değişen entity için migration
üretilir, oluşan SQL kullanıcıya gösterilir, onay sonrası gerçek
veritabanına (`VMWSCPADB\VIAAPP1` / `ViaCekek`) uygulanır. Şema tasarımı
tek taraflı yapılmaz.

## Teknoloji Yığını

- **.NET 8 (LTS)** — ASP.NET Core
- **Blazor Server** (Interactive Server render mode) — UI katmanı
  - Neden: Düşük/orta trafik (günde ~300 kişi) ve garanti edilen stabil ağ
    bağlantısı (bkz. Varsayımlar) göz önüne alındığında, Blazor Server'ın
    SignalR üzerinden sunduğu gerçek zamanlı UI güncellemesi (zamanlayıcı
    bazlı durum değişiklikleri, canlı liste güncellemeleri) ekstra JS/AJAX
    yazmaya gerek kalmadan doğal olarak gelir. Offline senaryo
    desteklenmeyeceği için Blazor Server'ın bağlantı-bağımlılığı bu projede
    dezavantaj oluşturmaz.
- **Entity Framework Core** — veri erişim katmanı, Code-First migration
  yaklaşımı (`dotnet ef migrations add ...` → SQL kullanıcıya gösterilir →
  onay sonrası `dotnet ef database update`)
- **SQL Server** — veritabanı
- **ASP.NET Core Identity** — kullanıcı adı/şifre ile kimlik doğrulama,
  rol bazlı yetkilendirme (örn. Güvenlik Görevlisi, Yönetici)
- **BackgroundService (IHostedService)** — periyodik durum güncelleme
  zamanlayıcısı için (örn. her dakika çalışıp süresi gelen ziyaret
  kayıtlarının durumunu güncelleyen arka plan görevi)

## Varsayımlar

- Tabletlerde/bilgisayarlarda ağ bağlantısı her zaman aktif kabul edilir;
  offline/kesinti senaryosu bu aşamada tasarlanmayacak.
- Tarayıcı: modern Chromium tabanlı tarayıcılar (Chrome/Edge) hedeflenir.

## Proje Yapısı

- `ViaCekek.sln` — çözüm dosyası (repo kökünde)
- `src/ViaCekek.Web/` — Blazor Web App (Interactive Server, Individual
  Identity auth ile scaffold edildi: `dotnet new blazor -au Individual
  -uld --empty`)
  - `Models/` — entity sınıfları
  - `Models/Common/AuditableEntity.cs` — ortak audit taban sınıfı (aşağıya
    bakınız)
  - `Data/ApplicationDbContext.cs` — EF Core DbContext, Identity +
    uygulama tabloları
  - `Data/Migrations/` — EF Core migration geçmişi

## Ortak Kurallar (Tüm Tablolar İçin)

- **Audit alanları**: Her tablo `KayitTarihi`, `Kaydeden`,
  `GuncellemeTarihi`, `Guncelleyen` alanlarına sahip olacak. Bu, ortak
  `AuditableEntity` taban sınıfından türetilerek sağlanır — yeni bir
  entity eklerken bu sınıftan türetmek yeterli, alanları tekrar tanımlamaya
  gerek yok.
- Bu alanlar `ApplicationDbContext.SaveChangesAsync` içinde otomatik
  doldurulur (ekleme → `KayitTarihi`/`Kaydeden`; güncelleme →
  `GuncellemeTarihi`/`Guncelleyen`). Kullanıcı adı, giriş yapmış kullanıcının
  `AuthenticationStateProvider` üzerinden okunan adıdır (Blazor Server'da
  `HttpContext` interactive circuit'te güvenilir olmadığı için bilerek
  `IHttpContextAccessor` **kullanılmıyor**). Senkron `SaveChanges` bilerek
  desteklenmiyor — tüm kayıt işlemleri `SaveChangesAsync` ile yapılmalı.

## Modüller (Geliştirme Sırası)

Modüller sırasıyla, birlikte kararlaştırılan sırayla geliştirilecek:

1. **Tekne Tanımları** ✅ — tekne kayıtlarının tutulduğu tablo/ekran
   (`Tekneler` tablosu ve migration'ları uygulandı; ekran/UI henüz yok)
2. **Kişi Tanımları** — kişi kayıtlarının tutulduğu tablo/ekran
3. **Araç Tanımları** — araç kayıtlarının tutulduğu tablo/ekran
4. **Belge Tanımları** — kontrol edilecek belge türlerinin parametrik
   tanımı ve hangi durumlarda sorgulanacaklarının kuralları
5. **Kişi Giriş/Çıkış Takip Ekranı** — tablette sürekli açık kalan ana
   operasyon ekranı
6. **Araç Giriş/Çıkış Takip Ekranı** — araçlar için benzer takip ekranı
7. **Zamanlayıcı / Durum Güncelleme Motoru** — parametrik kurallara göre
   ziyaret durumlarını otomatik güncelleyen arka plan servisi

## Veri Modeli (Kavramsal Taslak)

> Not: Aşağıdaki alan listelerine tüm tablolarda ortak olan audit alanları
> (`KayitTarihi`, `Kaydeden`, `GuncellemeTarihi`, `Guncelleyen` — bkz.
> "Ortak Kurallar") ayrıca yazılmamıştır, `AuditableEntity`'den otomatik
> gelir.

### Tekne (Boat)
- Tekne Kodu
- Tekne Adı
- Aktif / Pasif

### Kişi (Person)
- Kimlik Numarası (TC Kimlik No / Pasaport vb.)
- Ad Soyad
- Telefon
- Firma bilgisi (kişi bir firma personeliyse)
- Ziyaretçi Tipi (örn. Kaptan, Tekne Sahibi, Tekne Personeli, Diğer — bkz.
  "Ziyaretçi Tipleri" açık konusu)
- İlişkili Tekne(ler) — ziyaretçi tipi nitelikli (Kaptan/Tekne
  Sahibi/Tekne Personeli) ise hangi tekne(ler) ile ilişkili olduğu

### Araç (Vehicle)
- Kişi kaydına benzer temel bilgiler (plaka, marka/model vb. — netleşecek)
- Araç sahibi/kullanıcısı kişi ile ilişki

### Belge Tanımı (DocumentType) — Parametrik
- Belge Adı
- Geçerlilik süresi/tarihi kuralı
- Bu belgenin hangi durumlarda (ziyaretçi tipi, ziyaret tipi vb.)
  sorgulanacağını belirleyen kural(lar)

### Ziyaret / Giriş-Çıkış Kaydı (Visit)
- İlgili Kişi
- Ziyaret Sebebi
- Ziyaret Tipi (örn. Çalışma / Ziyaret / Diğer — netleşecek)
- KVKK Onay durumu (bkz. iş kuralları)
- Belge kontrol durumu/sonuçları
- Giriş Tarihi/Saati, işlemi yapan kullanıcı
- Çıkış Tarihi/Saati, işlemi yapan kullanıcı
- Durum (örn. Sahada / Çıkış Yaptı / Süresi Geçti vb. — netleşecek)
- İlişkili Araç (varsa)

## İş Kuralları

### Kişi Giriş Akışı
1. Kimlik numarası sorgulanır.
2. **Kayıt yoksa** → kişi kayıt ekranına yönlendirilir (yeni kişi
   oluşturma).
3. **Kayıt varsa** → süreç başlar. Pratiklik açısından kimlik numarası ve
   ziyaret sebebi aynı ekranda birlikte sorulabilir.
4. **KVKK onayı**: Kaptan ve Tekne Sahibi gibi istisnalar hariç, tüm
   ziyaretçilerden KVKK onayı takip edilir/istenir.
5. **Ziyaret tipi = Çalışma** ise, ek olarak gerekli belgelerin (belge
   tanımı tablosundaki parametrik kurallara göre) kontrolü yapılır.
6. Belge kontrolünden geçen kişi sahaya giriş yapar; giriş tarihi/saati ve
   işlemi yapan kullanıcı kaydedilir.

### Araç Giriş Akışı
- Kişi akışına benzer şekilde; araç bilgileri sorgulanır/kaydedilir ve
  giriş/çıkış takibi ayrı bir ekranda yapılır.

### Zamanlayıcı / Otomatik Durum Güncelleme
- Belirlenecek kurallara göre (örn. girişten 1 saat sonra durum
  güncellenir, sonraki güncelleme 15 dakika sonra tekrar yapılır gibi)
  ziyaret kayıtlarının durumu arka planda otomatik güncellenir.
- Bu kurallar parametrik olacak (sabit kod içine gömülmeyecek).

## Açık Konular / Netleştirilecekler

Proje başlamadan önce netleşmesi gereken, henüz karara bağlanmamış konular:

- **Ziyaretçi Tipleri**: Kaptan, Tekne Sahibi, Tekne Personeli dışında
  başka tipler olacak mı (örn. Misafir, Tedarikçi, Firma Personeli)?
- **Ziyaret Tipleri**: "Çalışma" dışında hangi ziyaret tipleri olacak,
  tam liste nedir?
- **KVKK onayı nasıl alınacak**: Tablette imza/onay ekranı mı, yoksa
  fiziksel form mu, yoksa tek tık onay mı?
- **Belge kontrol kuralları**: Belge–ziyaretçi tipi/ziyaret tipi
  eşleştirme kuralları tam olarak nasıl parametrize edilecek (matris
  ekranı, kural motoru vb.)?
- **Zamanlayıcı kuralları**: Durum güncelleme aralıkları (1 saat, 15 dk
  gibi örnekler verildi) ve bu güncellemelerin ne anlama geldiği (uyarı mı,
  otomatik çıkış mı, bildirim mi?) netleşmeli.
- **Kullanıcı rolleri**: Güvenlik görevlisi, yönetici gibi rollerin
  yetkileri (örn. kim yeni kişi/tekne/araç tanımlayabilir, kim raporlara
  erişebilir) netleşmeli.
- **Raporlama ihtiyaçları**: Sahada kimler var, geçmiş giriş/çıkış
  raporları gibi ihtiyaçlar var mı, varsa kapsamı nedir?
- **Tekne–Kişi ilişkisi çokluğu**: Bir kişi birden fazla tekneyle
  ilişkilendirilebilir mi (örn. bir kaptan birden fazla teknede
  çalışıyorsa)?

## Güvenlik / Gizli Bilgi Yönetimi

- Bağlantı dizeleri (connection string), şifreler, API anahtarları gibi
  gizli bilgiler **hiçbir zaman** kaynak koduna veya `appsettings.json`
  gibi Git'e dahil edilen dosyalara doğrudan yazılmayacak.
- Geliştirme ortamında **.NET User Secrets** (`dotnet user-secrets`)
  kullanılacak; production/staging ortamında ortam değişkenleri
  (environment variables) veya bir secret store (örn. Azure Key Vault,
  IIS/Windows üzerinde şifreli config) kullanılacak.
- `appsettings.json` içinde yalnızca gizli olmayan, ortam bağımsız
  varsayılan ayarlar bulunur; gizli değerler `appsettings.Development.json`,
  `appsettings.Production.json` gibi ortam bazlı dosyalara veya secret
  store'a taşınır ve bu dosyalar `.gitignore` ile hariç tutulur.
- Proje reposunda bir `.gitignore` bulunacak ve en az şunları
  içerecek: `appsettings.*.json` (Development/Production/Local
  varyantları), `*.env`, `secrets.json`, `bin/`, `obj/`, kullanıcıya özel
  IDE dosyaları.
- Repo'ya örnek/şablon bir `appsettings.Example.json` (gerçek değerler
  olmadan, sadece alan isimleriyle) eklenebilir; gerçek değerler her
  geliştirici/ortamda yerel olarak tutulur.
- Kod içinde yanlışlıkla gizli bilgi commit edilmesini önlemek için,
  commit öncesi bariz secret pattern'leri (şifre, connection string,
  API key) fark edilirse kullanıcıya uyarı verilecek.

## Notlar

- Bu dosya proje ilerledikçe modül modül güncellenecektir.
- Kod tabanı büyüdükçe bu dosyaya mimari kararlar, klasör yapısı ve
  geliştirme komutları eklenecektir.
