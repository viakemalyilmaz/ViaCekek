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
    `@attribute [Authorize]` + `@rendermode @(new InteractiveServerRenderMode(prerender: false))`
    (2026-08-11'den itibaren — düz `InteractiveServer` kısayolu değil,
    bkz. Zamanlayıcı bölümündeki "concurrent DbContext" notu)
  - `Components/App.razor` — **DENENDİ VE GERİ ALINDI (2026-08-06)**:
    `Home.razor`'da eksik olan `@rendermode InteractiveServer` yüzünden
    navbar toggler'ın tıklamaya yanıt vermemesi sorunu için önce
    `<HeadOutlet>`/`<Routes>`'a global `@rendermode="InteractiveServer"`
    denendi — ama bu, IIS'e yayınlanınca Login sayfasında **sonsuz
    yenileme döngüsüne** sebep oldu: Identity/Account bileşenleri
    (`AccountLayout` vb.) cookie yazabilmek için `HttpContext`'e ihtiyaç
    duyar ve bunun için **statik** render gerektirir; interaktif
    circuit'te çalışırken statik moda zorla geri dönmeye çalışır, global
    ayar da onu tekrar interaktif yapar → döngü. **Doğru çözüm**: global
    ayar geri alındı (`App.razor`'da artık `@rendermode` yok), bunun
    yerine yalnızca `Home.razor`'a diğer tüm sayfalardaki gibi sayfa
    bazlı `@rendermode InteractiveServer` eklendi. **Ders**: yeni bir
    sayfa eklenince `@rendermode InteractiveServer` eklemek unutulmamalı
    — global kısayol Identity/Account statik render gereksinimiyle
    çakışıyor, kullanılmamalı. (Not: `Home.razor` 2026-08-10'da silindi,
    `/` rotasını artık `Board.razor` kullanıyor — bkz. altta.)
  - `Components/Layout/MainLayout.razor` — üst navigasyon çubuğu (yeni
    modül eklendikçe nav-link buraya eklenir), `LoginDisplay.razor` —
    giriş/çıkış durumu
  - `wwwroot/bootstrap/` — Bootstrap CSS (proje `--empty` şablonla
    kurulduğu için CDN değil, `dotnet new blazor` çıktısından elle
    kopyalandı; offline/tablet güvenilirliği için CDN'e bağımlı değil)

## Tablet/Dokunmatik Uyumluluk (2026-08-06'da başladı)

- `wwwroot/app.css`'te proje geneli için: kök `font-size: 18px` (Bootstrap
  rem bazlı buton/boşluk ölçüleri orantılı büyür, her ekrana otomatik
  yayılır), büyütülmüş `.form-check-input` (checkbox) boyutu.
- Tüm tablolar `<div class="table-responsive">` ile sarmalanır (dar
  tablet ekranında yatay taşma yerine kaydırma).
- **Standart (2026-08-06'da tüm ekranlara yayıldı)**: önce sık
  kullanılacak Kişiler/Araçlar'da uygulanan düzen, kullanıcı isteğiyle
  **tüm tanım ekranlarına** (Tekneler, Kullanıcılar, Kişi Belgeleri, Araç
  Belgeleri dahil) aynı standartta yayıldı:
  - "+ Yeni X" ve Kaydet/Kapat butonları `btn-lg`; liste içindeki
    ikincil aksiyonlar (Düzenle, Şifre Sıfırla vb.) `btn-sm` değil
    normal boyut.
  - Form (yeni kayıt/düzenleme) açıkken alttaki liste **gizlenir**
    (`@if (!formAcik) { ... }`), sadece form kapalıyken gösterilir —
    kayıt/düzeltme ekranı gereksiz kalabalık olmadan tek işe odaklanır.
  - Formu kapatma butonu her durumda (yeni kayıt veya düzenleme) **"Kapat"**
    yazar — önceden yeni kayıtta "İptal" gösteriliyordu, 2026-08-06'da
    kullanıcı isteğiyle tüm ekranlarda tek bir metne (Kapat) sabitlendi.
    
- **Kişiler ve Araçlar formu kaydırma alanı (2026-08-07)**: form açıkken
  Kaydet/Kapat butonları ve kayıt sonucu bilgi/uyarı mesajı kaydırma
  alanının dışında, daima üstte kalır. Dikey scrollbar bu araç çubuğunun
  altındaki karttan başlar; yalnızca form alanları ve belge tablosu
  kaydırılır (`form-scroll-layout` + `form-scroll-content`). Aynı davranış `/kisiler` ve `/araclar` formlarına uygulanmıştır.
- Çekek Takip ekranı yapılırken bu prensipler baştan uygulanmalı: büyük
  dokunmatik butonlar, büyük yazı tipi, minimum tıklama, form açıkken
  liste/board gizli.
- **Üst navigasyon (2026-08-06'da düzeltildi)**: `navbar-expand` (breakpoint'siz)
  dar ekranda linklerin üst üste kırılmasına sebep oluyordu. `MainLayout.razor`
  artık `navbar-expand-lg` + gerçek bir daraltılabilir (hamburger) menü
  kullanıyor. Menü, Bootstrap JS veya Blazor event/state gerektirmeyen
  `<input type="checkbox">` + ilişkili `<label>` ve
  `.navbar-toggle-checkbox:checked ~ .navbar-collapse` CSS seçicisiyle
  açılıp kapanıyor; bu nedenle statik render edilen Identity/Account
  sayfalarında da çalışıyor. Menü sırası: Kişiler, Araçlar,
  Tekneler, Kişi Belgeleri, Araç Belgeleri, (Yönetici-only) Kullanıcılar.

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
- **Git commit/push** (2026-08-06'dan itibaren): her değişiklikten sonra
  otomatik commit/push yapılmaz — yalnızca kullanıcı açıkça isteyince.

## Sıradaki Adım (2026-08-12 itibarıyla)

Tüm tanım/kural ekranları, Çekek Takip (Kişi + Araç Girişi), Board ve
Çekek Takip Raporu tamamlandı (madde 1-6 aşağıda ✅). Kullanım kılavuzu
(docs/) da bu ekranları kapsayacak şekilde güncellendi.

Şu an açık/net bir sonraki adım yok — kullanıcı yeni konuları
belirleyecek (bkz. Açık Konular).

- ✅ **Çekek Takip Raporu tamamlandı (2026-08-12)**, `/rapor` (Yönetici +
  Saha Kontrolörü; Güvenlik erişemez — `MainLayout.razor`'da ayrı
  `AuthorizeView` grubunda). `CekekTakipRaporu.razor`, `@using
  CekekTakipKaydi = ViaCekek.Web.Models.CekekTakip` alias'ıyla (namespace
  çakışması için, bkz. Board notu) ve
  `@rendermode @(new InteractiveServerRenderMode(prerender: false))` ile.
  **Kapsam bilinçli basitleştirildi**: DB View yok, isimli/kayıtlı
  filtreler yok (farklı ihtiyaçlar için ayrı sabit raporlar üretilecek),
  Dynamic LINQ/ham SQL yok — sütunlar sabit olduğu için her filtre normal,
  tip-güvenli `.Where()` koşulu. Sütunlar: Kimlik No / Takip No (birlikte
  metin arama), Ad Soyad, Firma Adı, Telefon, **Araç Türü** (`Araclar`'a
  `Include` ile join, snapshot alanı değil), **Tekne** (`Tekneler`'e
  join), Ziyaret Sebebi, Giriş Tarihi/Saati, Çıkış Tarihi/Saati, **Durum**
  (metin karşılığı: Giriş Yapıldı/Süresi Geçti/Çıkış Yapıldı), Açıklama.
  **Kaydeden dahil değil** (kullanıcı istemiyor). Metin sütunları →
  içerir/eşittir/başlar ile; tarih sütunları → öncesi/sonrası/arası;
  Ziyaret Sebebi/Araç Türü/Tekne/Durum → dropdown. **Excel export**
  ClosedXML (v0.105.1, ücretsiz/MIT — EPPlus'ın yeni sürümleri ticari
  lisans gerektirdiği için tercih edilmedi) ile `XLWorkbook` oluşturup
  `MemoryStream`'e yazılıyor, base64'e çevrilip yeni
  `wwwroot/app.js`'teki `dosyaIndir()` (Blob + geçici `<a download>`)
  fonksiyonuna `IJSRuntime.InvokeVoidAsync` ile gönderiliyor — ayrı bir
  API endpoint'i açılmadı, proje tamamen Blazor component tabanlı kaldı.
  Gerçek DB'ye karşı scratch script'le uçtan uca doğrulandı: Kimlik/Takip
  No birleşik metin arama, Araç Türü join filtresi, Tekne filtresi, tarih
  "arası" aralık filtresi, Durum filtresi ve ClosedXML export'un
  (oluşturup geri okuyarak) doğru hücre değerleri ürettiği.

- ✅ **Araç Girişi tamamlandı (2026-08-10)**, aynı `/cekektakip`
  sayfasına eklendi (ayrı ekran değil). Aşama 1'e Kimlik No'nun yanına
  **Takip Numarası** eklendi — ikisi de opsiyonel, en az biri dolu
  olmalı, ikisi birden de girilebilir (kişi + araç birlikte). Kontrol:
  Kimlik No doluysa kişi zinciri, Takip No doluysa **bağımsız paralel**
  araç zinciri (kayıtlı mı → yasaklı/pasif mi → açık kaydı var mı →
  `AracTuru`'süne göre filtrelenmiş `AracBelgeleri` eksik mi — KVKK yok,
  ziyaret sebebinden bağımsız). **İkisi birden başarılı olmak zorunda
  değil** — hangisi geçerse Onay'a geçilir, yalnızca geçen(ler)
  kaydedilir; diğeri başarısızsa Onay ekranında bilgi amaçlı uyarı
  gösterilir. Onay: tek form (Ad Soyad kişi varsa salt okunur, Firma
  Adı/Telefon/Tekne/Açıklama ortak) — ikisi birden geçtiyse aynı
  (düzenlenmiş) Firma Adı/Telefon hem `Kisi` hem `Arac` kaydına senkron
  yazılır (Araç'ta Telefon alanı yok, sadece `CekekTakip` satırında
  bilgi amaçlı tutulur). Kimlik/Takip No alanlarının ikisinde de
  Kişiler/Araçlar ekranlarındaki gibi yazarken eşleşen kayıt arama var.
  `/araclar` ve `/kisiler`'e `?duzenle={id}` / `?yeniTakip=`/`?yeniKimlik=`
  query-string destekleri eklendi (Çekek Takip'teki "eksik belge" /
  "kayıtsız" yönlendirme linkleri için). Gerçek DB'ye karşı scratch
  script'lerle uçtan uca doğrulandı (araç-only, kombine kayıt, açık
  kayıt kontrolü, yasaklı araç).
- ✅ **Board ekranı tamamlandı (2026-08-10)**, uygulamanın **ana
  sayfası** (`@page "/"`, eski `Home.razor` silindi; sol üstteki logo
  linki zaten `/`'e gittiği için ayrıca değişiklik gerekmedi; navbar
  menüsünde ayrı bir "Board" linki yok, gerek kalmadı). Kart bazlı
  liste (tablo değil, tablette taranması daha kolay). Her kart bir
  aktif (`Durum != CikisYapildi`) `CekekTakip` satırı — Kişi ve Araç
  girişleri aynı listede birlikte görünür (ayrı satırlar olarak, birleş-
  tirilmiyor). Solda 48px'lik renkli daire ikon: mavi + insan silueti
  (kişi), turuncu + araç silueti (araç) — basit SVG primitifleriyle
  (circle/ellipse/rect/polygon), harici ikon kütüphanesi yok. Üstte
  arama kutusu (ad soyad/kimlik no/takip no/firma, canlı filtre). Sol
  kenarlık + rozet rengi: yeşil = normal, kırmızı = `ZamanAsimi`
  (`.board-karti-normal`/`.board-karti-kirmizi`, app.css); kalan süre
  rozeti ayrıca yeşil/mat sarı (10 dk veya altı)/kırmızı/gri (süresiz).
  **Sıralama**: süreli kayıtlar önce (kalan süresi en az/en çok geçmiş
  olan en üstte), süresiz kayıtlar en sonda (**ilk giren en üstte** —
  süreli grubun tersine). Sayfa her ziyaret edildiğinde (`ListeyiYenile`)
  süresi geçmiş ama hâlâ `GirisYapildi` olan kayıtlar `ZamanAsimi`'ye
  çevrilir — **ilk fazda otomatik/periyodik sayfa yenileme YOK**
  (kullanıcı isteğiyle bilinçli olarak basit tutuldu), sadece sayfa her
  açıldığında/işlem yapıldığında tazelenir. Her kartta **"+15 dk"**
  (yalnızca `Durum == ZamanAsimi` iken görünür — süresi henüz dolmamış
  kayıtlarda gösterilmez; basılınca yeni bitiş = **şu an + 15 dk**, eski
  geçmiş bitiş tarihinden değil; `ZamanAsimi`yı `GirisYapildi`ya
  resetler) ve **"Çıkış"** (ÇıkışTarihi/Saati yazar, Durum `CikisYapildi`
  olur, karttan kaybolur) butonları var. Gerçek DB'ye karşı scratch
  script'le doğrulandı (sıralama, otomatik durum geçişi, Çıkış, +15 dk).
- **Düzeltildi, tüm ekranlara yayıldı (2026-08-11)**: "A second operation
  was started on this context instance..." (concurrent DbContext) hatası
  — herhangi bir sayfa **tam sayfa yüklemesiyle** (URL'e doğrudan gidiş,
  yenileme, giriş sonrası yönlendirme — sadece SPA içi gezinme değil)
  açıldığında, prerender + interaktif circuit'in `OnInitializedAsync`'i
  aynı `DbContext` üzerinde çakışarak iki kez tetiklemesinden
  kaynaklanıyordu. Önce yalnızca Board'da (ana sayfa olduğu için ilk
  fark edilen yer) görüldü, sonra `/cekektakip`'te de aynı hata çıkınca
  bunun aslında **tüm sayfaları** etkileyen bir risk olduğu anlaşıldı.
  Çözüm tüm `Components/Pages/*.razor` dosyalarına uygulandı: sayfa
  düzeyindeki `@rendermode InteractiveServer` →
  `@rendermode @(new InteractiveServerRenderMode(prerender: false))`
  — prerender kapatıldı, `OnInitializedAsync` artık her sayfada yalnızca
  circuit bağlandığında bir kez çalışıyor. **Ders**: yeni bir sayfa
  eklenince de bu şekilde (prerender: false) yazılmalı, düz
  `InteractiveServer` kısayolu artık kullanılmamalı — Identity/Account
  sayfaları hâlâ istisna (bkz. App.razor notu, onlar hiç rendermode
  almamalı, statik kalmalı).
- **Karar (2026-08-11)**: Gerçek bir zamanlayıcı (`BackgroundService`)
  kurulmayacak — Board'un sayfa ziyaretinde tazeleme yapan mevcut hafif
  yaklaşımı kalıcı çözüm olarak kabul edildi, bu konu kapandı.

1. ✅ `KisiBelgeKontrolleri` şeması değişti: `KisiId` eklendi (zorunlu),
   `CekekTakipId` opsiyonel oldu, `UNIQUE(KisiId, KisiBelgeId)` eklendi
   — artık log değil, kişiye bağlı güncel durum tablosu. Migration
   uygulandı, gerçek DB'ye karşı (upsert + unique constraint + cascade
   delete) doğrulandı.
2. ✅ `/kisiler` ekranına belge girişi eklendi: form açılınca (yeni veya
   düzenleme) aktif `KisiBelgeleri` kurallarının tümü (ziyaret sebebiyle
   filtrelenmiyor, çünkü henüz bir ziyaret yok) Alındı checkbox + varsa
   Geçerlilik Tarihi ile, KVKK ilk satır olacak şekilde aynı tabloda
   listeleniyor. **Tek "Kaydet" butonu** kişi bilgilerini + belge
   durumlarını + KVKK'yı aynı `SaveChangesAsync` çağrısında kaydediyor
   (2026-08-06'da iki ayrı buton — "Kaydet" ve "Belgeleri Kaydet" — tek
   butona birleştirildi). Yeni kişi için EF Core'un navigation-based
   ilişki eşleştirmesi kullanılıyor (`KisiBelgeKontrol.Kisi = kisi`),
   çünkü kişinin Id'si SaveChanges'ten önce bilinmiyor — bu davranış
   gerçek DB'ye karşı ayrıca doğrulandı.
3. ✅ `AracBelgeKontrolleri` için aynı şema değişikliği (`AracId` eklendi,
   `CekekTakipId` opsiyonel oldu, `UNIQUE(AracId, AracBelgeId)` eklendi),
   `/araclar` ekranına aynı belge girişi bölümü **tek Kaydet butonuyla**
   eklendi. Kişi'den fark: `AracBelgeleri` kuralları o aracın kendi
   `AracTuru`süne göre (`GecerliArac`/`GecerliVinc`/...) otomatik
   filtreleniyor (tüm kurallar değil) — Araç Türü dropdown'u değişince
   liste `@bind-Value:after` ile yeniden yükleniyor. Navigation-fixup
   davranışı (yeni araç + belgeleri tek `SaveChangesAsync`'te) ve
   AracTuru filtre sorgusu gerçek DB'ye karşı ayrıca doğrulandı.
   "Belgeler" başlığı (Kişi ve Araç ekranlarında) kaldırıldı — tablonun
   "Belge" sütun başlığıyla zaten redundandı.
4. ✅ Çekek Takip — Kişi Girişi (`/cekektakip`, Aşama 1: Sorgu + Onay)
   tamamlandı, commit'lendi (2026-08-07, `4e75a87`). Kontrol sırası:
   kayıtlı mı → yasaklı/pasif mi → açık (çıkışsız) kaydı var mı →
   eksik/süresi geçmiş belge var mı (KVKK dahil) → Onay aşamasında
   Ad Soyad salt okunur, Firma Adı/Telefon/Tekne/Açıklama düzenlenebilir,
   Kaydet sonrası Firma/Telefon kişi kaydına da yazılır, form otomatik
   sorgu ekranına döner. Okuma sorguları `AsNoTracking` kullanır (başka
   sekmede yapılan güncellemelerin anında görülmesi için). Gerçek DB'ye
   karşı scratch script'lerle uçtan uca doğrulandı.
5. ✅ Çekek Takip — Araç Girişi eklendi (2026-08-10) — bkz. yukarıdaki
   "Sıradaki Adım" bölümü.
6. ✅ Board ekranı eklendi (2026-08-10), uygulamanın ana sayfası
   (`@page "/"`, eski `Home.razor` silindi) — Kişi ve Araç girişlerini
   birlikte gösterir. Gerçek zamanlayıcı (`BackgroundService`)
   kurulmayacağına karar verildi (2026-08-11) — bu iş kapandı.

## Modüller (Geliştirme Sırası)

Not: Kişi ve Araç giriş/çıkış takibi tek bir ekranda (Çekek Takip)
birleştirildi — aşağıdaki 5. madde eski "Kişi Giriş/Çıkış" ve "Araç
Giriş/Çıkış" maddelerinin yerine geçer. Ekran sırası 2026-08-05'te
kullanıcı tercihiyle değişti: Araç Tanımları ertelendi, önce Kişi
Belgeleri yapıldı — Araç ve Araç Belgeleri ekranları sonraki adımlar.

1. **Tekne Tanımları** ✅ — `Tekneler` tablosu + `/tekneler` ekranı
   (liste + ekle/düzenle) tamamlandı, kullanıcı testi bekleniyor
2. **Kişi Tanımları** ✅ — `Kisiler` tablosu + `/kisiler` ekranı (liste +
   ekle/düzenle, Firma Adı autocomplete, koşullu Yasaklanma Sebebi alanı,
   belge girişi bölümü — tek "Kaydet" ile kişi + belgeler + KVKK aynı
   anda `KisiBelgeKontrolleri`'ne upsert) tamamlandı, kullanıcı testi
   bekleniyor
3. **Kişi Belgeleri** ✅ — `KisiBelgeleri` tablosu + `/kisibelgeleri`
   ekranı (kural tanımı: belge adı, kontrol kuralı, ziyaret sebebi
   uygulanabilirliği) tamamlandı, kullanıcı testi bekleniyor
4. **Araç Tanımları** ✅ — `Araclar` tablosu + `/araclar` ekranı (liste +
   ekle/düzenle, Araç Türü seçimi, Firma Adı autocomplete, koşullu
   Yasaklanma Sebebi, araç türüne göre filtrelenmiş belge girişi bölümü
   — tek "Kaydet" ile araç + belgeler aynı anda `AracBelgeKontrolleri`'ne
   upsert) tamamlandı, kullanıcı testi bekleniyor
5. **Araç Belgeleri** ✅ — `AracBelgeleri` tablosu + `/aracbelgeleri`
   ekranı (kural tanımı: belge adı, kontrol kuralı, araç türü
   uygulanabilirliği) tamamlandı, kullanıcı testi bekleniyor
6. **Çekek Takip Ekranı** ✅ — Kişi + Araç Girişi (`/cekektakip`), Board
   (`/`) ve Çekek Takip Raporu (`/rapor`) tamamlandı (bkz. Sıradaki Adım).
7. **Zamanlayıcı / Durum Güncelleme Motoru** — gerçek bir
   `BackgroundService` kurulmayacağına karar verildi (2026-08-11, bkz.
   Açık Konular); Board'un sayfa ziyaretinde tazeleme yapan hafif
   yaklaşımı kalıcı çözüm. Bu madde kapandı, ayrı bir iş kalmadı.

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
    `GecerliKontrol`, `GecerliMalzemeAlma`, `GecerliMalzemeBirakma`,
    `GecerliIskeleKurma` (bkz. Ziyaret Sebebi enum'u)

### Belge Durumu — `KisiBelgeKontrolleri` / `AracBelgeKontrolleri` (ikisi de uygulandı, 2026-08-06)

İkisi de artık log değil, kişiye/araca bağlı **güncel durum** tablosu —
bir kişi/araç × bir belge kuralı = tek satır, upsert edilir. Alanlar:
`KisiId`/`AracId` (zorunlu FK), `KisiBelgeId`/`AracBelgeId` (zorunlu FK),
`CekekTakipId` (**opsiyonel** — yalnızca bir Çekek Takip girişinde
teyit/güncelleme yapılırsa doldurulur, kayıt anında boş kalır),
`AlindiSonucu` (bool), `GecerlilikTarihiSonucu` (nullable DateTime).
`UNIQUE(KisiId, KisiBelgeId)` / `UNIQUE(AracId, AracBelgeId)` kısıtı var.
Doldurma yeri: `/kisiler` ve `/araclar` ekranlarındaki belge tablosu
(başlıksız — tablonun "Belge" sütunu yeterli, ayrı "Belgeler" etiketi
2026-08-06'da kaldırıldı). Yeni kayıt + belgelerin aynı `SaveChangesAsync`
çağrısında kaydedilmesi EF Core'un navigation-based ilişki eşleştirmesiyle
çözüldü (`KisiBelgeKontrol.Kisi = kisi` / `AracBelgeKontrol.Arac = arac`).

**Kural (2026-08-06):** Geçerlilik Tarihi alanı, Alındı işaretli değilken
UI'da devre dışı (`disabled`). Kaydet anında, Alındı işaretsizken hâlâ bir
tarih varsa (örn. önce girilip sonra Alındı kaldırılmışsa) tarih otomatik
`null`'a çevrilir ve kullanıcıya hangi belge(ler) için bunun yapıldığını
söyleyen bir uyarı (`alert-warning`) gösterilir — kayıt engellenmez, sadece
uyarılır. Aynı kural KVKK Onay Tarihi için de geçerli.

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
  Malzeme Alma, Malzeme Bırakma, İskele Kurma
- `Durum` — sabit enum (2026-08-07 eklendi): `GirisYapildi`,
  `ZamanAsimi`, `CikisYapildi`, varsayılan `GirisYapildi`
- `BeklenenBitisZamani` — nullable DateTime (2026-08-07 eklendi),
  Ziyaret Sebebi'ne göre süre dolacağı an; süresiz sebeplerde null;
  board'da öncelik sıralaması ve "+15 dk" uzatma için kullanılır
- `Aciklama` — serbest metin

## İş Kuralları

### Çekek Takip Akışı — Kişi Girişi (2026-08-07'de netleşen plan — henüz UYGULANMADI)

Amaç: kontrolden geçenlerin girişini **hızlıca** kaydetmek. Bu ekranda
belge girişi YAPILMAZ, sadece kontrol edilir. **Tek sayfa** (`/cekektakip`),
diğer tüm ekranlardaki liste↔form deseniyle aynı mantıkta **iki aşamalı**:
Sorgu aşaması → Onay aşaması (ayrı bir route değil, aynı sayfada durum
değişimiyle — sayfa yenilenmez, hız korunur).

**Aşama 1 — Sorgu**: Kimlik Numarası + Ziyaret Sebebi (radio) girilir,
"Kontrol" butonuna basılır. Kontrol sırayla yapılır, ilk başarısız olan
durdurur ve uyarı gösterir:

1. **Kişi kayıtlı mı?** Değilse → uyarı + "+ Yeni Kişi" ekranına
   yönlendirme linki, giriş yapılmaz.
2. **Yasaklı mı?** (Aktif=false VE Yasaklanma Sebebi dolu) → "Saha
   kontrolüne haber verilmeli, yasaklı kişi tespit edildi" uyarısı
   (en öncelikli/spesifik durum — bu kontrol "pasif mi" kontrolünden
   önce yapılır, çünkü `YasaklanmaSebebi` yalnızca `Aktif=false` iken
   dolu olabilir, aksi sırada hiç tetiklenmez), giriş yapılmaz.
3. **Pasif mi?** (Aktif=false, Yasaklanma Sebebi boş) → genel "kişi
   aktif değil" uyarısı, giriş yapılmaz.
4. **Eksik/süresi geçmiş belge var mı?** `KisiBelgeleri` kurallarına göre
   (aktif + ziyaret sebebine uygulanabilir olanlar) kontrol edilir. KVKK
   da bir belge gibi ele alınır — tüm ziyaret sebeplerinde geçerli,
   yalnızca `KvkkOnayFormuAlindi` bool'una bakılır (`KvkkOnayDurumu`/
   `KvkkOnayTarihi` bu kapı kontrolünde kullanılmaz). Eksik/süresi geçmiş
   varsa → hangileri olduğu uyarı olarak listelenir + "Kişi Tanımları"na
   yönlendirme linki, giriş yapılmaz.
5. Hepsi geçtiyse → Aşama 2'ye geçilir.

**Aşama 2 — Onay**: 
- **Ad Soyad**: salt okunur (Kişi kaydından), yalnızca `/kisiler`
  ekranından değiştirilebilir.
- **Firma Adı, Telefon**: düzenlenebilir — değişmiş olabilir. Kaydet'te
  hem `CekekTakipleri`ye (o günkü anlık görüntü) hem de ilgili `Kisi`
  kaydına yazılır (kişi kaydı da güncellenmiş olur).
- **Tekne**: opsiyonel dropdown (`CekekTakip.TekneId`).
- Kaydet → `CekekTakipleri`ye giriş satırı düşülür (GirişTarihi/
  GirişSaati = şimdi, ZiyaretSebebi Aşama 1'den, snapshot alanları +
  TekneId, işlemi yapan kullanıcı audit alanlarından), ardından
  otomatik olarak Aşama 1'in boş sorgu ekranına dönülür (hızlı art arda
  giriş için).

**Çıkış**: Bu ekrandan değil, ayrı **board ekranından** (satırda bir
"Çıkış" butonu ile) yapılacak.

**Araç girişi**: Aynı `CekekTakipleri` tablosuna yazacak ama kontrol
sırası/alanları kişiden **farklı olacak** — ayrıca konuşulup
netleştirilecek, bu plan yalnızca KİŞİ tarafını kapsar.

Eski planlar (referans, artık geçerli değil): "belge kontrolü de Çekek
Takip sırasında her girişte yeniden yapılır" ve "tek aşamalı ekran,
eksik belgede doğrudan Kişi Tanımları'na yönlendirilip geri dönülmez"
varsayımları kullanıcıyla konuşulup yukarıdaki akışa değiştirildi.

### Zamanlayıcı / Otomatik Durum Güncelleme
- **Süre kuralları netleşti (2026-08-07)**: Ziyaret Sebebi'ne göre
  içeride kalma süresi — Çalışma/İskele Kurma/Görüşme = **süresiz**,
  Keşif/Kontrol = **1 saat**, Malzeme Alma/Malzeme Bırakma = **15
  dakika**. Süresi dolan kayıtlar board'da öncelik artırılarak en üste
  çıkar; kullanıcı board üzerinden "+15 dk" uzatma verebilir.
- **Şema eklendi (2026-08-07, migration uygulandı)**: `CekekTakipleri`ye
  `Durum` (enum: `GirisYapildi`/`ZamanAsimi`/`CikisYapildi`, varsayılan
  `GirisYapildi`) ve `BeklenenBitisZamani` (nullable DateTime, süresiz
  sebeplerde null; giriş anında Ziyaret Sebebi'ne göre hesaplanıp
  yazılır, "+15 dk" bu alanı ileri alır) eklendi. `GirisYapildi` →
  `ZamanAsimi` geçişi **şimdilik gerçek bir arka plan servisiyle değil**,
  board ekranı her açıldığında/yenilendiğinde süresi geçmiş kayıtları
  güncelleyen hafif bir sorguyla yapılacak — tam zamanlayıcı
  (`BackgroundService`) bu modülden ayrı, sonraki bir adımda ele
  alınacak (bkz. Modüller > 7).

## Açık Konular / Netleştirilecekler

- **Raporlama ihtiyaçları**: Sahada kimler var, geçmiş giriş/çıkış
  raporları gibi ihtiyaçlar var mı, varsa kapsamı nedir? Kullanıcı bilgi
  verecek, henüz gelmedi — tek açık konu bu.

Çözülenler (referans için): Ziyaret Tipleri → `ZiyaretSebebi` sabit
enum'u ile netleşti (Çalışma/Görüşme/Keşif/Kontrol/Malzeme Alma/Malzeme
Bırakma/İskele Kurma). Belge kontrol kuralları → `KisiBelgeleri`/`AracBelgeleri`
üzerindeki checkbox'larla parametrize edildi. Tekne–Kişi ilişkisi →
kalıcı bir alan yerine her girişte `CekekTakip.TekneId` ile çözüldü.
Kullanıcı rolleri → 3 sabit rol ile netleşti (bkz. Kullanıcı Yönetimi).
**KVKK onayı** (2026-08-11) → ayrı bir imza/form akışına gerek yok,
mevcut belge-kontrolü yaklaşımı (Alındı checkbox, Kişi ekranında) yeterli
kabul edildi, ek iş yok. **Zamanlayıcı** (2026-08-11) → gerçek bir
`BackgroundService` kurulmayacak, Board'un sayfa ziyaretinde tazeleme
yapan hafif yaklaşımı (bkz. Zamanlayıcı bölümü) kalıcı çözüm olarak
kabul edildi. **Araç–Kişi ilişkisi** (2026-08-11) → kalıcı bir
sahiplik/kullanıcı ilişkisi modellenmeyecek, kapsam dışı bırakıldı.
**Yasaklı kişi/araç uyarısı** (2026-08-11) → Çekek Takip ekranındaki
mevcut uyarı yeterli kabul edildi, bildirim vb. ek bir mekanizma
istenmiyor. **Araca özgü belge detayları** (2026-08-11) → zaten
`AracBelgeleri`nin `AracTuru`'ne göre filtrelemesiyle karşılanıyor, ayrı
bir iş kalmadı.

## Kullanıcı Yönetimi (uygulandı, 2026-08-05)

- 3 sabit rol (`Data/Roller.cs`): **Yönetici** (her şey: tanım ekranları +
  Çekek Takip + Kullanıcılar), **Saha Kontrolörü** (tanım ekranları +
  Çekek Takip, Kullanıcılar hariç), **Güvenlik** (Kişiler, Araçlar ve
  Çekek Takip — Tekneler, Kişi/Araç Belgeleri, Kullanıcılar hariç;
  2026-08-07'de Kişiler/Araçlar erişimi eklendi, sahada bu iki ekranı
  kullanacaklar).
- Roller ve `kemalyilmaz@viadmc.com` → Yönetici ataması, uygulama her
  başladığında `Program.cs`'de idempotent olarak garanti edilir (yoksa
  oluşturulur, varsa dokunulmaz).
- **Kayıt formu artık herkese açık değil**: `/Account/Register`
  `[Authorize(Roles = Roller.Yonetici)]` ile korunuyor. Ancak bu sayfa
  kaydı yapan kişiyi otomatik oturum açtırdığı için (kendi kaydını yapan
  kullanıcı için tasarlanmış) yönetici tarafından **kullanılmamalı** —
  onun yerine `/kullanicilar` ekranındaki "Yeni Kullanıcı" formu
  kullanılır (yöneticinin oturumunu bozmadan `UserManager.CreateAsync` +
  `AddToRoleAsync` ile doğrudan hesap açar).
- **Kullanıcı Adı / Ad Soyad ayrımı (2026-08-11)**: `ApplicationUser`'a
  `Name` (Ad Soyad, nullable, migration ile eklendi) eklendi. Artık
  `UserName` (giriş için) ve `Email` (yalnızca iletişim bilgisi, artık
  girişte kullanılmıyor) birbirinden bağımsız — önceden `UserName`
  her zaman `Email` ile zorunlu aynıydı. **Giriş ekranı** (`Login.razor`)
  artık "E-posta" değil **"Kullanıcı Adı"** ile çalışıyor
  (`SignInManager.PasswordSignInAsync(Input.KullaniciAdi, ...)` —
  bu API zaten her zaman UserName'e göre arıyordu, önceden UserName=Email
  olduğu için email gibi görünüyordu). Mevcut (eski) kullanıcılara
  dokunulmadı — onların `UserName`'i hâlâ email formatında, girişte
  aynı şekilde yazmaya devam ederler; `Name` alanları boş, isteyen
  Düzenle ile sonradan doldurur. Audit alanları (Kaydeden/Guncelleyen)
  `Identity.Name` → `UserName`'den geldiği için bu değişiklikten
  etkilenmedi (davranış aynı, sadece artık UserName email olmak zorunda
  değil). Gerçek DB'ye karşı scratch script'le uçtan uca doğrulandı
  (oluşturma, kullanıcı adıyla arama/şifre kontrolü, email'in artık
  kullanıcı adı yerine geçmediği, düzenleme sonrası UserName'in sabit
  kaldığı).
- **E-posta bilerek tekil değil (2026-08-11)**: Birden fazla kullanıcı
  aynı e-postayı paylaşabilir (örn. bir ekibin ortak e-postasıyla farklı
  Kullanıcı Adı'na sahip birkaç hesabı olması — gerçek kullanım deseni).
  Bu yüzden `Program.cs`'teki ilk yönetici kontrolü `FindByEmailAsync`
  (tekil sonuç bekler, birden fazla eşleşmede istisna fırlatır ve
  **uygulamayı başlangıçta çökertir**) yerine `FindByNameAsync`
  kullanır — `UserName` Identity tarafından zaten tekil garanti edilir.
  Aynı sebeple kullanılmayan `RegisterConfirmation.razor`'daki
  `FindByEmailAsync` çağrısı da `NormalizeEmail` + `FirstOrDefaultAsync`
  ile dayanıklı hale getirildi. `ForgotPassword`, `ResetPassword` (+
  onay sayfaları) ve `ResendEmailConfirmation` sayfaları — zaten hiçbir
  yerden linklenmiyordu ve e-posta gönderimi no-op olduğu için işlevsel
  değillerdi — bu aynı riski taşıdıkları için tamamen silindi. Gerçek
  DB'ye karşı (paylaşılan e-postalı gerçek kayıtlarla) doğrulandı.
- `/kullanicilar` (Yönetici-only): kullanıcı listesi (Kullanıcı Adı, Ad
  Soyad, Rol, Durum) + tek bir form hem **yeni kullanıcı oluşturma** hem
  **düzenleme** için kullanılır (diğer ekranlarla aynı desen): "Düzenle"
  butonu formu o kullanıcının Kullanıcı Adı'yla (salt okunur, değişmez),
  Ad Soyad, E-posta, Rol ve Aktif değerleriyle açar.
  **Aktif/Pasif** (2026-08-06) şema değişikliği gerektirmeden ASP.NET
  Core Identity'nin hazır lockout mekanizmasıyla uygulandı:
  `UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue)` →
  Pasif (giriş engellenir, `SignInManager` zaten `IsLockedOut`u kontrol
  edip `LockedOut` sonucu döner), `SetLockoutEndDateAsync(user, null)` →
  Aktif. Liste ekranında "Aktif" durumu `LockoutEnd is null ||
  LockoutEnd <= UtcNow` ile hesaplanır. Şifre Sıfırla ayrı bir aksiyon
  olarak kaldı (Kaydet formunun bir parçası değil).
- `Routes.razor`: yetkisiz ama zaten giriş yapmış bir kullanıcı artık
  login sayfasına döngüye girmiyor, "Bu sayfaya erişim yetkiniz yok"
  mesajı görüyor.
- **Login ekranı sadeleştirildi**: dış servisle giriş bölümü, "Register as
  a new user" ve "Resend email confirmation" linkleri kaldırıldı (hiçbiri
  bu projede kullanılmıyor/çalışmıyor). "Forgot your password?" de
  kaldırıldı — gerçek e-posta sunucusu olmadığı için hiç çalışmayacaktı;
  yerine "şifrenizi unuttuysanız yöneticinizle iletişime geçin" notu
  kondu. Karşılığında `/kullanicilar` ekranına her kullanıcı satırında
  **Şifre Sıfırla** özelliği eklendi (`UserManager.ResetPasswordAsync`,
  e-postaya ihtiyaç duymadan Yönetici doğrudan yeni şifre belirler).

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
