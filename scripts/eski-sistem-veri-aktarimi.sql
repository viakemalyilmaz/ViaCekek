/* ============================================================================
   ESKİ SİSTEM (eflow) -> YENİ SİSTEM (ViaCekek) VERİ AKTARIM SCRIPT'İ
   ============================================================================
   Hazırlanma tarihi: 2026-08-25

   VARSAYIM: Eski (eflow) ve yeni (ViaCekek) veritabanları AYNI SQL Server
   instance'ında duruyor, bu yüzden script üç parçalı isimlendirme kullanıyor
   ([eflow].[dbo].[Tablo] / [ViaCekek].[dbo].[Tablo]). Farklı sunucudaysalar,
   önce bir linked server kurup üç parçalı adları dört parçalıya
   ([LinkedServerAdi].[eflow].[dbo].[Tablo]) çevirmeniz gerekir.

   NASIL ÇALIŞTIRILIR:
   1) Önce "ADIM 0" bölümündeki tanılama sorgularını çalıştırıp GERÇEK
      verideki farklı (distinct) değerleri gözünüzle kontrol edin.
   2) Aşağıdaki "⚠️ DOĞRULA" işaretli CASE/eşleştirme blokları, o gerçek
      değerlere göre GEREKİRSE düzeltin (bkz. her adımın başındaki not).
   3) Adımları SIRAYLA, teker teker çalıştırın (hepsini birden değil) —
      her adımdan sonra `SELECT COUNT(*)` ile beklenen satır sayısını
      kontrol edin.
   4) Script'in tamamı **idempotent**: `WHERE NOT EXISTS` kullanıyor, yani
      aynı adımı yanlışlıkla iki kez çalıştırırsanız veri ikilenmez
      (doğal anahtarlarla — KimlikNumarasi/TakipNumarasi/TekneKodu —
      kontrol ediyor).
   5) Hiçbir adım otomatik yedek almıyor — çalıştırmadan önce ViaCekek
      veritabanının bir yedeğini (backup) almanız önerilir.

   ÖNEMLİ BULGU (gerçek DB'ye bakılarak tespit edildi):
   - Yeni sistemde `Tekneler` tablosunda hâlâ **6664 satır** var — bu,
     eski `CekekTekneKayit` tablosundan çok daha büyük, yani Tekneler
     muhtemelen ZATEN başka/daha büyük bir kaynaktan aktarılmış.
     ADIM 1 (Tekneler) bu yüzden muhtemelen gereksiz olabilir — çalıştırmadan
     önce gerçekten gerekli olup olmadığını değerlendirin.
   - `Kisiler` (31), `Araclar` (11), `CekekTakipleri` (98) satırlarında hâlâ
     az miktarda test/gerçek veri var — script bunlarla çakışmayı doğal
     anahtar (KimlikNumarasi/TakipNumarasi) üzerinden `NOT EXISTS` ile
     engelliyor.
   - `KisiBelgeleri` (11 satır) ve `AracBelgeleri` (2 satır) tanımları,
     eski sistemdeki inline belge alanlarıyla **birebir isim eşleşmesi**
     yapıyor (aşağıda ADIM 4/5'te kullanılıyor) — bu iyi haber, ekstra
     eşleştirme tablosu gerekmedi.
   ============================================================================ */


/* ============================================================================
   ADIM 0 — TANILAMA: eşleştirme yapmadan önce gerçek değerleri görün
   ============================================================================
   Bu bölüm yalnızca SELECT yapar, hiçbir şey değiştirmez. Aşağıdaki
   adımlardaki CASE/eşleştirme mantığını bu sonuçlara göre gözden geçirin. */

-- Eski Araç Türü serbest metin değerleri (ADIM 3'teki eşleştirme için):
SELECT DISTINCT AracTuru, COUNT(*) AS adet
FROM [eflow].[dbo].[CekekAracKayit]
GROUP BY AracTuru;

-- Eski Ziyaretçi Tipi serbest metin değerleri (ADIM 2'deki
-- Kaptan/TekneSahibi/TeknePersoneli eşleştirmesi için):
SELECT DISTINCT ZiyaretciTipi, COUNT(*) AS adet
FROM [eflow].[dbo].[CekekKisiKayit]
GROUP BY ZiyaretciTipi;

-- Eski KVKK Onay serbest metin değerleri (ADIM 2'deki KvkkOnayDurumu için):
SELECT DISTINCT kvkkOnay, COUNT(*) AS adet
FROM [eflow].[dbo].[CekekKisiKayit]
GROUP BY kvkkOnay;

-- Eski Tekne Aktif kodu (ADIM 1'deki E/H varsayımı için — DF_CekekTekneKayit_Aktif
-- default'u 'H' idi, muhtemelen H=Hayır/E=Evet ama gerçek değerlerle doğrulayın):
SELECT DISTINCT Aktif, COUNT(*) AS adet
FROM [eflow].[dbo].[CekekTekneKayit]
GROUP BY Aktif;

-- Eski Ziyaret Sebebi serbest metin değerleri (ADIM 6'daki ZiyaretSebebi
-- enum eşleştirmesi için):
SELECT DISTINCT Ziyaret_sebebi, COUNT(*) AS adet
FROM [eflow].[dbo].[CekekTakip]
GROUP BY Ziyaret_sebebi;

-- Belge alanlarında "alındı" anlamına gelen değerler ne yazıyor? (örnek 20 satır)
SELECT TOP 20 sorumlulukYazisi, sgkHizmetDokumu, isgEgitimBelgesi, kvkkOnayFormu
FROM [eflow].[dbo].[CekekKisiKayit]
WHERE sorumlulukYazisi IS NOT NULL OR kvkkOnayFormu IS NOT NULL;


/* ============================================================================
   ADIM 1 — Tekneler  (kaynak: CekekTekneKayit)
   ⚠️ Yukarıdaki not: yeni sistemde zaten 6664 satır var, bu adımın gerekip
   gerekmediğini önce değerlendirin. Doğal anahtar: TekneKodu.
   ⚠️ DOĞRULA: Aktif kodu 'E'=Evet / diğer her şey = Hayır varsayıldı.
   ============================================================================ */
INSERT INTO [ViaCekek].[dbo].[Tekneler]
    (TekneKodu, TekneAdi, Aktif, KayitTarihi, Kaydeden)
SELECT
    LTRIM(RTRIM(e.TekneNumarasi))                                   AS TekneKodu,
    LTRIM(RTRIM(e.TekneAdi))                                        AS TekneAdi,
    CASE WHEN UPPER(LTRIM(RTRIM(e.Aktif))) = N'E' THEN 1 ELSE 0 END AS Aktif,
    GETDATE()                                                       AS KayitTarihi,
    N'eski-sistem-aktarim'                                          AS Kaydeden
FROM [eflow].[dbo].[CekekTekneKayit] e
WHERE e.TekneNumarasi IS NOT NULL
  AND LTRIM(RTRIM(e.TekneNumarasi)) <> ''
  AND NOT EXISTS (
        SELECT 1 FROM [ViaCekek].[dbo].[Tekneler] t
        WHERE t.TekneKodu = LTRIM(RTRIM(e.TekneNumarasi))
      );

-- Kontrol:
-- SELECT COUNT(*) FROM [ViaCekek].[dbo].[Tekneler];


/* ============================================================================
   ADIM 2 — Kişiler  (kaynak: CekekKisiKayit)
   Doğal anahtar: KimlikNumarasi (= eski tckn).
   ⚠️ DOĞRULA: ZiyaretciTipi ve kvkkOnay eşleştirmeleri aşağıda LIKE ile
   esnek tutuldu (büyük/küçük harf ve Türkçe karakter farklarına karşı) —
   ADIM 0'daki gerçek distinct değerlerle karşılaştırıp gerekirse LIKE
   kalıplarını güncelleyin.
   ============================================================================ */
INSERT INTO [ViaCekek].[dbo].[Kisiler]
    (KimlikNumarasi, AdSoyad, FirmaAdi, Telefon, Aktif, YasaklanmaSebebi,
     TekneSahibi, Kaptan, TeknePersoneli,
     KvkkOnayFormuAlindi, KvkkOnayDurumu, KvkkOnayTarihi,
     KayitTarihi, Kaydeden, GuncellemeTarihi, Guncelleyen)
SELECT
    LTRIM(RTRIM(e.tckn))                                             AS KimlikNumarasi,
    LTRIM(RTRIM(e.adSoyad))                                          AS AdSoyad,
    NULLIF(LTRIM(RTRIM(e.firmaismi)), '')                            AS FirmaAdi,
    NULLIF(
        -- Telefon: rakam olmayan karakterleri (boşluk, tire, parantez, +) temizler.
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
            LTRIM(RTRIM(e.gsm)), ' ', ''), '-', ''), '(', ''), ')', ''), '+', ''),
        '')                                                          AS Telefon,
    ISNULL(e.isAktif, 1)                                             AS Aktif,
    CASE WHEN ISNULL(e.isAktif, 1) = 0
         THEN NULLIF(LTRIM(RTRIM(e.bansebebi)), '')
         ELSE NULL END                                               AS YasaklanmaSebebi,

    -- ⚠️ DOĞRULA: gerçek ZiyaretciTipi değerlerine göre kalıpları düzeltin.
    CASE WHEN e.ZiyaretciTipi LIKE N'%sahib%' OR e.ZiyaretciTipi LIKE N'%sahip%'
         THEN 1 ELSE 0 END                                           AS TekneSahibi,
    CASE WHEN e.ZiyaretciTipi LIKE N'%kaptan%'
         THEN 1 ELSE 0 END                                           AS Kaptan,
    CASE WHEN e.ZiyaretciTipi LIKE N'%personel%'
         THEN 1 ELSE 0 END                                           AS TeknePersoneli,

    CASE WHEN NULLIF(LTRIM(RTRIM(e.kvkkOnayFormu)), '') IS NOT NULL
         THEN 1 ELSE 0 END                                           AS KvkkOnayFormuAlindi,

    -- ⚠️ DOĞRULA: gerçek kvkkOnay değerlerine göre kalıpları düzeltin.
    CASE WHEN e.kvkkOnay LIKE N'%verildi%' OR e.kvkkOnay LIKE N'%onayland%'
         THEN N'OnayVerildi'
         WHEN e.kvkkOnay LIKE N'%verilmedi%' OR e.kvkkOnay LIKE N'%onaylanmad%' OR e.kvkkOnay LIKE N'%red%'
         THEN N'OnayVerilmedi'
         ELSE N'Bilinmiyor' END                                      AS KvkkOnayDurumu,
    e.kvkkOnayFormuTarih                                             AS KvkkOnayTarihi,

    ISNULL(e.kayittarihi, GETDATE())                                 AS KayitTarihi,
    N'eski-sistem-aktarim'                                           AS Kaydeden,
    e.guncellemetarihi                                               AS GuncellemeTarihi,
    CASE WHEN e.guncellemetarihi IS NOT NULL
         THEN N'eski-sistem-aktarim' ELSE NULL END                   AS Guncelleyen
FROM [eflow].[dbo].[CekekKisiKayit] e
WHERE e.tckn IS NOT NULL
  AND LTRIM(RTRIM(e.tckn)) <> ''
  AND NOT EXISTS (
        SELECT 1 FROM [ViaCekek].[dbo].[Kisiler] k
        WHERE k.KimlikNumarasi = LTRIM(RTRIM(e.tckn))
      );

-- Kontrol:
-- SELECT COUNT(*) FROM [ViaCekek].[dbo].[Kisiler];


/* ============================================================================
   ADIM 3 — Araçlar  (kaynak: CekekAracKayit)
   Doğal anahtar: TakipNumarasi.
   ⚠️ DOĞRULA: AracTuru serbest metni enum'a LIKE ile eşleştirildi — ADIM 0
   sonucuna göre kalıpları düzeltin. Eşleşmeyenler varsayılan olarak 'Arac'
   (Araç) oluyor.
   ⚠️ ÖNEMLİ VARSAYIM: eski tabloda Aktif alanı YOK, yalnızca BanSebebi var.
   Aktif = "BanSebebi doluysa Pasif, boşsa Aktif" olarak TÜRETİLDİ — bu veri
   değil, çıkarım; kontrol edin.
   ============================================================================ */
INSERT INTO [ViaCekek].[dbo].[Araclar]
    (TakipNumarasi, AracTuru, FirmaAdi, Aktif, YasaklanmaSebebi,
     KayitTarihi, Kaydeden)
SELECT
    LTRIM(RTRIM(e.TakipNumarasi))                                    AS TakipNumarasi,

    -- ⚠️ DOĞRULA: gerçek AracTuru değerlerine göre kalıpları düzeltin.
    CASE WHEN e.AracTuru LIKE N'%vinç%' OR e.AracTuru LIKE N'%vinc%' THEN N'Vinc'
         WHEN e.AracTuru LIKE N'%vidanj%' THEN N'Vidanjor'
         WHEN e.AracTuru LIKE N'%kompres%' THEN N'Kompresor'
         WHEN e.AracTuru LIKE N'%basınç%' OR e.AracTuru LIKE N'%basinc%' THEN N'BasincliKap'
         ELSE N'Arac' END                                            AS AracTuru,

    NULLIF(LTRIM(RTRIM(e.FirmaAdi)), '')                             AS FirmaAdi,

    -- Aktif alanı eski tabloda yok — BanSebebi doluysa Pasif kabul edildi.
    CASE WHEN NULLIF(LTRIM(RTRIM(e.BanSebebi)), '') IS NOT NULL
         THEN 0 ELSE 1 END                                           AS Aktif,
    NULLIF(LTRIM(RTRIM(e.BanSebebi)), '')                            AS YasaklanmaSebebi,

    GETDATE()                                                        AS KayitTarihi,
    N'eski-sistem-aktarim'                                           AS Kaydeden
FROM [eflow].[dbo].[CekekAracKayit] e
WHERE e.TakipNumarasi IS NOT NULL
  AND LTRIM(RTRIM(e.TakipNumarasi)) <> ''
  AND NOT EXISTS (
        SELECT 1 FROM [ViaCekek].[dbo].[Araclar] a
        WHERE a.TakipNumarasi = LTRIM(RTRIM(e.TakipNumarasi))
      );

-- Kontrol:
-- SELECT COUNT(*) FROM [ViaCekek].[dbo].[Araclar];


/* ============================================================================
   ADIM 4 — Kişi Belge Kontrolleri  (kaynak: CekekKisiKayit'in inline
   belge alanları -> KisiBelgeKontrolleri, KisiBelgeleri.BelgeTanimi ile
   BİREBİR isim eşleşmesi kullanılıyor, ekstra eşleştirme gerekmedi).

   Not: Alanı boş/NULL olan kişi-belge çiftleri için satır eklenmiyor —
   uygulama zaten kayıt yoksa "Alınmadı" olarak gösteriyor (bkz.
   CLAUDE.md > Kişi Belgeleri), o yüzden migration'da "boş" satırlar
   üretmeye gerek yok.
   ============================================================================ */
INSERT INTO [ViaCekek].[dbo].[KisiBelgeKontrolleri]
    (KisiId, KisiBelgeId, AlindiSonucu, GecerlilikTarihiSonucu)
SELECT k.Id, b.Id, x.AlindiSonucu, x.GecerlilikTarihiSonucu
FROM (
    SELECT tckn, N'İskele Kurma' AS BelgeTanimi,
           CASE WHEN NULLIF(LTRIM(RTRIM(iskeleKurmaBelgesi)), '') IS NOT NULL THEN 1 ELSE 0 END AS AlindiSonucu,
           iskeleKurmaBelgesiTarih AS GecerlilikTarihiSonucu
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(iskeleKurmaBelgesi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Giriş Talimatnamesi',
           CASE WHEN NULLIF(LTRIM(RTRIM(GirisTalimatname)), '') IS NOT NULL THEN 1 ELSE 0 END,
           NULL  -- bu belgede GecerlilikTarihiKontrolu=False, tarih tutulmuyor
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(GirisTalimatname)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Sorumluluk Yazısı',
           CASE WHEN NULLIF(LTRIM(RTRIM(sorumlulukYazisi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           sorumlulukYazisiTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(sorumlulukYazisi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Sgk Hizmet Dökümü',
           CASE WHEN NULLIF(LTRIM(RTRIM(sgkHizmetDokumu)), '') IS NOT NULL THEN 1 ELSE 0 END,
           sgkHizmetDokumuTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(sgkHizmetDokumu)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'İsg Eğitim Belgesi',
           CASE WHEN NULLIF(LTRIM(RTRIM(isgEgitimBelgesi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           isgEgitimBelgesiTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(isgEgitimBelgesi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Sağlık Raporu',
           CASE WHEN NULLIF(LTRIM(RTRIM(saglikRaporu)), '') IS NOT NULL THEN 1 ELSE 0 END,
           saglikRaporuTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(saglikRaporu)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Görevlendirme Yazısı',
           CASE WHEN NULLIF(LTRIM(RTRIM(gorevlendirmeYazisi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           gorevlendirmeYazisiTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(gorevlendirmeYazisi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Mesleki Yeterlilik Sertifikası',
           CASE WHEN NULLIF(LTRIM(RTRIM(meslekiYeterlilikSertifikasi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           meslekiYeterlilikSertifikasiTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(meslekiYeterlilikSertifikasi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Yüksekte Çalışma Belgesi',
           CASE WHEN NULLIF(LTRIM(RTRIM(yuksekteCalismaBelgesi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           yuksekteCalismaBelgesiTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(yuksekteCalismaBelgesi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Adli Sicil Kaydı',
           CASE WHEN NULLIF(LTRIM(RTRIM(adliSicilKaydi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           adliSicilKaydiTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(adliSicilKaydi)), '') IS NOT NULL

    UNION ALL
    SELECT tckn, N'Kkd Zimmet Formu',
           CASE WHEN NULLIF(LTRIM(RTRIM(kkdZimmetFormu)), '') IS NOT NULL THEN 1 ELSE 0 END,
           kkdZimmetFormuTarih
    FROM [eflow].[dbo].[CekekKisiKayit] WHERE NULLIF(LTRIM(RTRIM(kkdZimmetFormu)), '') IS NOT NULL
) x
JOIN [ViaCekek].[dbo].[Kisiler] k ON k.KimlikNumarasi = LTRIM(RTRIM(x.tckn))
JOIN [ViaCekek].[dbo].[KisiBelgeleri] b ON b.BelgeTanimi = x.BelgeTanimi
WHERE NOT EXISTS (
        SELECT 1 FROM [ViaCekek].[dbo].[KisiBelgeKontrolleri] mevcut
        WHERE mevcut.KisiId = k.Id AND mevcut.KisiBelgeId = b.Id
      );

-- Kontrol:
-- SELECT COUNT(*) FROM [ViaCekek].[dbo].[KisiBelgeKontrolleri];


/* ============================================================================
   ADIM 5 — Araç Belge Kontrolleri  (kaynak: CekekAracKayit'in inline
   PeriyodikMuayeneFormu / TrafikSigortasi alanları)
   ============================================================================ */
INSERT INTO [ViaCekek].[dbo].[AracBelgeKontrolleri]
    (AracId, AracBelgeId, AlindiSonucu, GecerlilikTarihiSonucu)
SELECT a.Id, b.Id, x.AlindiSonucu, x.GecerlilikTarihiSonucu
FROM (
    SELECT TakipNumarasi, N'Periyodik Muayene Formu' AS BelgeTanimi,
           CASE WHEN NULLIF(LTRIM(RTRIM(PeriyodikMuayeneFormu)), '') IS NOT NULL THEN 1 ELSE 0 END AS AlindiSonucu,
           PeriyodikMuayeneTarihi AS GecerlilikTarihiSonucu
    FROM [eflow].[dbo].[CekekAracKayit] WHERE NULLIF(LTRIM(RTRIM(PeriyodikMuayeneFormu)), '') IS NOT NULL

    UNION ALL
    SELECT TakipNumarasi, N'Trafik Sigortası',
           CASE WHEN NULLIF(LTRIM(RTRIM(TrafikSigortasi)), '') IS NOT NULL THEN 1 ELSE 0 END,
           TrafikSigortasiTarihi
    FROM [eflow].[dbo].[CekekAracKayit] WHERE NULLIF(LTRIM(RTRIM(TrafikSigortasi)), '') IS NOT NULL
) x
JOIN [ViaCekek].[dbo].[Araclar] a ON a.TakipNumarasi = LTRIM(RTRIM(x.TakipNumarasi))
JOIN [ViaCekek].[dbo].[AracBelgeleri] b ON b.BelgeTanimi = x.BelgeTanimi
WHERE NOT EXISTS (
        SELECT 1 FROM [ViaCekek].[dbo].[AracBelgeKontrolleri] mevcut
        WHERE mevcut.AracId = a.Id AND mevcut.AracBelgeId = b.Id
      );

-- Kontrol:
-- SELECT COUNT(*) FROM [ViaCekek].[dbo].[AracBelgeKontrolleri];


/* ============================================================================
   ADIM 6 — Çekek Takip geçmişi  (kaynak: CekekTakip) — EN DÜŞÜK GÜVENİLİRLİK

   ⚠️ Bu adım diğerlerinden daha riskli/varsayıma dayalı, çünkü:
   - Eski tabloda bir satırda HEM kişi HEM araç bilgisi birlikte olabiliyor;
     yeni şemada bir CekekTakip satırı ya kişi ya araçtır (CHECK constraint).
     Bu yüzden eski satır ikisini de doluysa YENİ SİSTEMDE İKİ AYRI SATIR
     üretiliyor (aşağıda iki ayrı INSERT bloğu var) — tıpkı uygulamanın
     "kombine giriş" akışında yaptığı gibi.
   - Ziyaret_sebebi serbest metin -> sabit enum'a LIKE ile eşleştirildi,
     ADIM 0 sonucuna göre kalıpları doğrulayın. Eşleşmeyenler 'Gorusme'
     (Görüşme) varsayılan oluyor.
   - Arac_plaka, yeni Araclar.TakipNumarasi ile eşleşmeyebilir (format
     farkı olabilir) — eşleşmeyen satırlar için AracId bulunamaz ve o
     satır atlanır (araç tarafı migrate edilmez, sadece kişi tarafı
     migrate edilir, ya da tam tersi).
   - BeklenenBitisZamani ve Durum=ZamanAsimi geçmişe dönük hesaplanmıyor;
     tüm geçmiş kayıtlar ya "GirisYapildi" (hâlâ açık, ÇıkışTarihi boşsa)
     ya da "CikisYapildi" (ÇıkışTarihi doluysa) olarak işaretleniyor.
   - Bu tablo yalnızca RAPORLAMA/geçmiş amaçlıdır, güncel iş kurallarını
     etkilemez — isterseniz bu adımı hiç çalıştırmayabilirsiniz.
   ============================================================================ */

-- 6a) Kişi tarafı (Tckn doluysa)
INSERT INTO [ViaCekek].[dbo].[CekekTakipleri]
    (KisiId, KimlikNumarasi, AdSoyad, FirmaAdi, Telefon,
     GirisTarihi, GirisSaati, CikisTarihi, CikisSaati,
     ZiyaretSebebi, Durum, Aciklama, KayitTarihi, Kaydeden)
SELECT
    k.Id,
    LTRIM(RTRIM(e.Tckn)),
    LTRIM(RTRIM(ISNULL(e.Ad, '') + ' ' + ISNULL(e.Soyad, ''))),
    NULLIF(LTRIM(RTRIM(e.Firma_ismi)), ''),
    NULLIF(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(e.Telefon_numarasi)), ' ', ''), '-', ''), '+', ''), ''),
    CAST(e.GirisTarihi AS date),
    CAST(e.GirisTarihi AS time),
    CAST(e.CikisTarihi AS date),
    CAST(e.CikisTarihi AS time),
    -- ⚠️ DOĞRULA: gerçek Ziyaret_sebebi değerlerine göre kalıpları düzeltin.
    CASE WHEN e.Ziyaret_sebebi LIKE N'%çalışma%' OR e.Ziyaret_sebebi LIKE N'%calisma%' THEN N'Calisma'
         WHEN e.Ziyaret_sebebi LIKE N'%keşif%' OR e.Ziyaret_sebebi LIKE N'%kesif%' THEN N'Kesif'
         WHEN e.Ziyaret_sebebi LIKE N'%kontrol%' THEN N'Kontrol'
         WHEN e.Ziyaret_sebebi LIKE N'%malzeme%al%' THEN N'MalzemeAlma'
         WHEN e.Ziyaret_sebebi LIKE N'%malzeme%bırak%' OR e.Ziyaret_sebebi LIKE N'%malzeme%birak%' THEN N'MalzemeBirakma'
         WHEN e.Ziyaret_sebebi LIKE N'%iskele%' THEN N'IskeleKurma'
         ELSE N'Gorusme' END,
    CASE WHEN e.CikisTarihi IS NOT NULL THEN 2 ELSE 0 END,  -- 2=CikisYapildi, 0=GirisYapildi
    NULLIF(LTRIM(RTRIM(e.Aciklama)), ''),
    ISNULL(e.GirisTarihi, GETDATE()),
    N'eski-sistem-aktarim'
FROM [eflow].[dbo].[CekekTakip] e
JOIN [ViaCekek].[dbo].[Kisiler] k ON k.KimlikNumarasi = LTRIM(RTRIM(e.Tckn))
WHERE e.Tckn IS NOT NULL
  AND LTRIM(RTRIM(e.Tckn)) <> ''
  AND e.GirisTarihi IS NOT NULL
  AND NOT EXISTS (
        -- Aynı kişi + aynı giriş anı zaten aktarılmışsa tekrar eklenmesin.
        SELECT 1 FROM [ViaCekek].[dbo].[CekekTakipleri] mevcut
        WHERE mevcut.KisiId = k.Id
          AND mevcut.GirisTarihi = CAST(e.GirisTarihi AS date)
          AND mevcut.GirisSaati = CAST(e.GirisTarihi AS time)
      );

-- 6b) Araç tarafı (Arac_plaka doluysa VE Araclar'da eşleşen bir TakipNumarasi varsa)
INSERT INTO [ViaCekek].[dbo].[CekekTakipleri]
    (AracId, TakipNumarasi, FirmaAdi, Telefon,
     GirisTarihi, GirisSaati, CikisTarihi, CikisSaati,
     ZiyaretSebebi, Durum, Aciklama, KayitTarihi, Kaydeden)
SELECT
    a.Id,
    LTRIM(RTRIM(e.Arac_plaka)),
    NULLIF(LTRIM(RTRIM(e.Firma_ismi)), ''),
    NULLIF(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(e.Telefon_numarasi)), ' ', ''), '-', ''), '+', ''), ''),
    CAST(e.GirisTarihi AS date),
    CAST(e.GirisTarihi AS time),
    CAST(e.CikisTarihi AS date),
    CAST(e.CikisTarihi AS time),
    CASE WHEN e.Ziyaret_sebebi LIKE N'%çalışma%' OR e.Ziyaret_sebebi LIKE N'%calisma%' THEN N'Calisma'
         WHEN e.Ziyaret_sebebi LIKE N'%keşif%' OR e.Ziyaret_sebebi LIKE N'%kesif%' THEN N'Kesif'
         WHEN e.Ziyaret_sebebi LIKE N'%kontrol%' THEN N'Kontrol'
         WHEN e.Ziyaret_sebebi LIKE N'%malzeme%al%' THEN N'MalzemeAlma'
         WHEN e.Ziyaret_sebebi LIKE N'%malzeme%bırak%' OR e.Ziyaret_sebebi LIKE N'%malzeme%birak%' THEN N'MalzemeBirakma'
         WHEN e.Ziyaret_sebebi LIKE N'%iskele%' THEN N'IskeleKurma'
         ELSE N'Gorusme' END,
    CASE WHEN e.CikisTarihi IS NOT NULL THEN 2 ELSE 0 END,
    NULLIF(LTRIM(RTRIM(e.Aciklama)), ''),
    ISNULL(e.GirisTarihi, GETDATE()),
    N'eski-sistem-aktarim'
FROM [eflow].[dbo].[CekekTakip] e
JOIN [ViaCekek].[dbo].[Araclar] a ON a.TakipNumarasi = LTRIM(RTRIM(e.Arac_plaka))
WHERE e.Arac_plaka IS NOT NULL
  AND LTRIM(RTRIM(e.Arac_plaka)) <> ''
  AND e.GirisTarihi IS NOT NULL
  AND NOT EXISTS (
        SELECT 1 FROM [ViaCekek].[dbo].[CekekTakipleri] mevcut
        WHERE mevcut.AracId = a.Id
          AND mevcut.GirisTarihi = CAST(e.GirisTarihi AS date)
          AND mevcut.GirisSaati = CAST(e.GirisTarihi AS time)
      );

-- Kontrol:
-- SELECT COUNT(*) FROM [ViaCekek].[dbo].[CekekTakipleri];

-- Eşleşmeyen (Araclar'da karşılığı bulunamayan) Arac_plaka değerlerini görmek isterseniz:
-- SELECT DISTINCT e.Arac_plaka
-- FROM [eflow].[dbo].[CekekTakip] e
-- WHERE e.Arac_plaka IS NOT NULL AND LTRIM(RTRIM(e.Arac_plaka)) <> ''
--   AND NOT EXISTS (SELECT 1 FROM [ViaCekek].[dbo].[Araclar] a WHERE a.TakipNumarasi = LTRIM(RTRIM(e.Arac_plaka)));
