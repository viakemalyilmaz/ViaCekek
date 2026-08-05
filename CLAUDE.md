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
  - `Components/Pages/` — ekranlar (örn. `Tekneler.razor`), her biri
    `@attribute [Authorize]` + `@rendermode InteractiveServer`
  - `Components/Layout/MainLayout.razor` — üst navigasyon çubuğu (yeni
    modül eklendikçe nav-link buraya eklenir), `LoginDisplay.razor` —
    giriş/çıkış durumu
  - `wwwroot/bootstrap/` — Bootstrap CSS (proje `--empty` şablonla
    kurulduğu için CDN değil, `dotnet new blazor` çıktısından elle
    kopyalandı; offline/tablet güvenilirliği için CDN'e bağımlı değil)

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

## Geliştirme Süreci

- Şema değişiklikleri: migration üretilir → SQL kullanıcıya gösterilir →
  onay sonrası `dotnet ef database update` ile uygulanır (bkz. Proje
  Özeti).
- **Ekranlar** (2026-08-05'ten itibaren): bir ekranın UI'ı bitip build
  temiz geçince kullanıcıya haber verilir, kullanıcı tarayıcıda kendisi
  test eder, onay sonrası sıradaki modüle geçilir. Birden fazla ekran
  arka arkaya, teste açılmadan inşa edilmez.

## Modüller (Geliştirme Sırası)

Not: Kişi ve Araç giriş/çıkış takibi tek bir ekranda (Çekek Takip)
birleştirildi — aşağıdaki 5. madde eski "Kişi Giriş/Çıkış" ve "Araç
Giriş/Çıkış" maddelerinin yerine geçer.

1. **Tekne Tanımları** ✅ — `Tekneler` tablosu + `/tekneler` ekranı
   (liste + ekle/düzenle) tamamlandı, kullanıcı testi bekleniyor
2. **Kişi Tanımları** ✅ (tablo) — `Kisiler`; ekran/UI henüz yok
3. **Araç Tanımları** ✅ (tablo) — `Araclar`; ekran/UI henüz yok
4. **Belge Tanımları** ✅ (tablo) — `KisiBelgeleri`, `AracBelgeleri`
   (kural/kontrol listesi tabloları); ekran/UI henüz yok
5. **Çekek Takip Ekranı** ✅ (tablo) — `CekekTakipleri` (hem kişi hem
   araç giriş/çıkışı tek tabloda); tablette sürekli açık kalan ana
   operasyon ekranı; ekran/UI henüz yok
6. **Zamanlayıcı / Durum Güncelleme Motoru** — parametrik kurallara göre
   giriş/çıkış kayıtlarının durumunu otomatik güncelleyen arka plan
   servisi; henüz başlanmadı

## Veri Modeli (Kavramsal Taslak)

> Not: Aşağıdaki alan listelerine tüm tablolarda ortak olan audit alanları
> (`KayitTarihi`, `Kaydeden`, `GuncellemeTarihi`, `Guncelleyen` — bkz.
> "Ortak Kurallar") ayrıca yazılmamıştır, `AuditableEntity`'den otomatik
> gelir.

### Tekne (Boat)
- Tekne Kodu
- Tekne Adı
- Aktif / Pasif

### Kişi (Person) — uygulandı (`Kisiler` tablosu)
- Kimlik Numarası — `nvarchar(20)`, **unique** (TC Kimlik No / Pasaport,
  sabit uzunluk/format zorunlu değil)
- Ad Soyad — tek alan (Ad/Soyad ayrı tutulmuyor, hız önceliği)
- Firma Adı — serbest metin, ayrı bir Firma tablosu **yok** (hız
  önceliği); UI'da önceden girilmiş `FirmaAdi` değerlerinden autocomplete
  önerilecek (indexlendi)
- Telefon — yalnızca rakam, boşluksuz, 7-15 hane (TR formatına
  kısıtlanmadı, yurt dışı numaraları da girilebilir)
- Aktif / Pasif
- Yasaklanma Sebebi — nullable; yalnızca Pasif iken doldurulabilir
  (veritabanı seviyesinde CHECK constraint ile garanti altına alındı).
  Kişi Takip Ekranı (modül 5) yapılırken: bu alan doluysa sorgulama
  anında görünür bir uyarı gösterilecek.
- Ziyaretçi niteliği: `TekneSahibi`, `Kaptan`, `TeknePersoneli` —
  birbirini dışlamayan bağımsız checkbox'lar (bir kişi aynı anda birden
  fazlası olabilir)
- KVKK: `KvkkOnayFormuAlindi` (bool), `KvkkOnayDurumu` (enum: Bilinmiyor/
  OnayVerildi/OnayVerilmedi), `KvkkOnayTarihi` (nullable) — yalnızca
  güncel durum tutulur, değişiklik geçmişi audit alanlarından
  (Guncelleyen/GuncellemeTarihi) izlenir
- Tekne ilişkisi kalıcı bir alan olarak **yok** — hangi tekneyle ilgili
  olduğu her girişte `CekekTakip.TekneId` ile ayrıca tutulur (bkz. altta)

### Araç (Vehicle) — uygulandı (`Araclar` tablosu)
- Takip Numarası — `nvarchar(50)`, **unique** (plaka veya vinç/vidanjör/
  kompresör/basınçlı kap gibi ekipmanlar için farklı bir seri no olabilir,
  sabit format zorunlu değil)
- Araç Türü — sabit tanımlı enum: `Arac` (Araç), `Vinc` (Vinç),
  `Vidanjor` (Vidanjör), `Kompresor` (Kompresör), `BasincliKap`
  (Basınçlı Kap); veritabanında metin olarak saklanır
- Firma Adı, Aktif/Pasif, Yasaklanma Sebebi — Kişi tablosuyla birebir
  aynı yaklaşım (serbest metin + autocomplete index; CHECK constraint ile
  Yasaklanma Sebebi yalnızca Pasif iken doldurulabilir)
- **Henüz yok**: araç–kişi ilişkisi (sahibi/kullanıcısı) — netleşecek

### Belge Kuralları — `KisiBelgeleri` ve `AracBelgeleri` (uygulandı)

Kişi ve Araç için birebir aynı mantıkta, birbirinden bağımsız iki tablo.
Bunlar **kişiye/araca değil kurala ait** tablolardır — "hangi belge, ne
zaman, nasıl kontrol edilir" tanımını tutar; gerçek kontrol sonuçları
ayrı tablolarda (altta).

- Belge Tanımı — serbest metin (nvarchar), ayrı bir lookup tablosu yok
- `Alindi` (bool) ve `GecerlilikTarihiKontrolu` (bool): bunlar VERİ değil
  **kontrol kuralıdır** — "bu belge için alındı kontrolü yapılsın mı" /
  "ayrıca geçerlilik tarihi de kontrol edilsin mi" anlamına gelir
- Aktif/Pasif
- Uygulanabilirlik checkbox'ları (birbirini dışlamaz):
  - `AracBelgeleri`: `GecerliArac`, `GecerliVinc`, `GecerliVidanjor`,
    `GecerliKompresor`, `GecerliBasincliKap` (bkz. Araç Türü enum'u)
  - `KisiBelgeleri`: `GecerliCalisma`, `GecerliGorusme`, `GecerliKesif`,
    `GecerliKontrol`, `GecerliMalzemeAlma`, `GecerliMalzemeBirakma`
    (bkz. Ziyaret Sebebi enum'u)

### Belge Kontrol Logları — `KisiBelgeKontrolleri` / `AracBelgeKontrolleri` (uygulandı)

Kural tablolarının aksine bunlar **gerçek kontrol olaylarının kaydıdır**
(log): her `CekekTakip` girişinde ilgili belgeler kontrol edildikçe yeni
satır eklenir, üzerine yazılmaz (tekillik kısıtı yok).

- `CekekTakipId` — hangi giriş sırasında kontrol yapıldığı
- `KisiBelgeId` / `AracBelgeId` — hangi kurala göre kontrol edildiği
- `AlindiSonucu` (bool), `GecerlilikTarihiSonucu` (nullable DateTime) —
  kontrolde bulunan gerçek sonuçlar

### Çekek Takip — `CekekTakipleri` (uygulandı)

Hem kişi hem araç giriş/çıkışları **tek tabloda** tutulur.

- `KisiId` / `AracId` — nullable FK, bir satırda ikisinden **tam biri**
  dolu olur (CHECK constraint ile garanti edilir)
- Giriş anı **anlık görüntüsü** (snapshot): `KimlikNumarasi`,
  `TakipNumarasi`, `AdSoyad`, `FirmaAdi`, `Telefon` — Kisi/Arac kaydı
  sonradan değişse bile o günkü kayıt bozulmaz
- `TekneId` — nullable FK, bu girişin hangi tekneyle ilgili olduğu
  (kalıcı Kişi–Tekne ilişkisi yerine her girişte ayrı ayrı tutulur)
- `GirisTarihi`/`GirisSaati`, `CikisTarihi`/`CikisSaati` — tarih ve saat
  ayrı kolonlar (`date`/`time`)
- `ZiyaretSebebi` — sabit enum: Çalışma, Görüşme, Keşif, Kontrol,
  Malzeme Alma, Malzeme Bırakma
- `Aciklama` — serbest metin

## İş Kuralları

### Kişi Giriş Akışı (Çekek Takip ekranı)
1. Kimlik numarası sorgulanır (`Kisiler.KimlikNumarasi`).
2. **Kayıt yoksa** → kişi kayıt ekranına yönlendirilir (yeni kişi
   oluşturma).
3. **Kayıt varsa** → süreç başlar. Pratiklik açısından kimlik numarası ve
   ziyaret sebebi aynı ekranda birlikte sorulabilir.
4. **KVKK onayı**: Kaptan ve Tekne Sahibi gibi istisnalar hariç, tüm
   ziyaretçilerden `Kisi.KvkkOnayFormuAlindi`/`KvkkOnayDurumu` üzerinden
   takip edilir/istenir.
5. Seçilen `ZiyaretSebebi`ye göre, `KisiBelgeleri` tablosunda o sebep için
   işaretli (`GecerliCalisma` vb.) ve aktif olan belgeler bulunur, her
   biri için `KisiBelgeKontrolleri`ne bir kontrol kaydı düşülür (Alındı
   ve varsa Geçerlilik Tarihi kontrolü).
6. Belge kontrolünden geçen kişi için `CekekTakipleri`de yeni bir satır
   açılır: giriş tarihi/saati ve işlemi yapan kullanıcı (audit alanları
   üzerinden) kaydedilir.

### Araç Giriş Akışı (Çekek Takip ekranı — aynı tablo, aynı ekran)
- Kişi akışına birebir paralel: Takip Numarası sorgulanır
  (`Araclar.TakipNumarasi`), araç türüne (`AracTuru`) göre
  `AracBelgeleri`nde işaretli belgeler bulunur, `AracBelgeKontrolleri`ne
  kontrol kaydı düşülür, sonra `CekekTakipleri`de yeni satır açılır.
- Kişi ve araç girişleri **aynı `CekekTakipleri` tablosunda**, ayrı
  satırlar olarak tutulur (bkz. Veri Modeli).

### Zamanlayıcı / Otomatik Durum Güncelleme
- Belirlenecek kurallara göre (örn. girişten 1 saat sonra durum
  güncellenir, sonraki güncelleme 15 dakika sonra tekrar yapılır gibi)
  `CekekTakipleri` kayıtlarının durumu arka planda otomatik güncellenir.
- Bu kurallar parametrik olacak (sabit kod içine gömülmeyecek). Henüz
  `CekekTakipleri`de bir "Durum" alanı yok — bu motor tasarlanırken
  eklenecek.

## Açık Konular / Netleştirilecekler

Proje başlamadan önce netleşmesi gereken, henüz karara bağlanmamış konular:

- **KVKK onayı nasıl alınacak**: Tablette imza/onay ekranı mı, yoksa
  fiziksel form mu, yoksa tek tık onay mı?
- **Zamanlayıcı kuralları**: Durum güncelleme aralıkları (1 saat, 15 dk
  gibi örnekler verildi) ve bu güncellemelerin ne anlama geldiği (uyarı mı,
  otomatik çıkış mı, bildirim mi?) netleşmeli. `CekekTakipleri`de henüz
  bir "Durum" alanı yok.
- **Kullanıcı rolleri**: Güvenlik görevlisi, yönetici gibi rollerin
  yetkileri (örn. kim yeni kişi/tekne/araç tanımlayabilir, kim raporlara
  erişebilir) netleşmeli.
- **Raporlama ihtiyaçları**: Sahada kimler var, geçmiş giriş/çıkış
  raporları gibi ihtiyaçlar var mı, varsa kapsamı nedir?
- **Araç–Kişi ilişkisi**: Bir aracın sahibi/kullanıcısı olan kişi nasıl
  tutulacak (tek kişi mi, birden fazla mı)? Henüz modellenmedi.

Çözülenler (referans için): Ziyaret Tipleri → `ZiyaretSebebi` sabit
enum'u ile netleşti (Çalışma/Görüşme/Keşif/Kontrol/Malzeme Alma/Malzeme
Bırakma). Belge kontrol kuralları → `KisiBelgeleri`/`AracBelgeleri`
üzerindeki checkbox'larla parametrize edildi. Tekne–Kişi ilişkisi →
kalıcı bir alan yerine her girişte `CekekTakip.TekneId` ile çözüldü.

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
