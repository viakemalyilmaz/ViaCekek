/* ============================================================================
   YENİ SİSTEM (ViaCekek.CekekTakipleri) -> ESKİ SİSTEM (eflow.CekekTakip)
   ŞEMASINDA SONUÇ DÖNDÜREN SORGU
   ============================================================================
   Hazırlanma tarihi: 2026-09-30

   eski-sistem-veri-aktarimi.sql > ADIM 6'nın TERSİ: yeni sistemdeki giriş/
   çıkış kayıtlarını, eski CekekTakip tablosunun kolon adları/biçimiyle
   döndürür. Yalnızca SELECT — hiçbir şey değiştirmez.

   DÖNÜŞÜM KURALLARI:
   - Tarih + saat: yeni sistemde ayrı kolonlar (date + time), eski sistemde
     tek datetime -> GirisTarihi+GirisSaati birleştirilir.
   - Ad / Soyad: yeni sistemde tek AdSoyad alanı var. Son kelime Soyad,
     geri kalanı Ad kabul edildi (aktarımda Ad + ' ' + Soyad birleştirilmişti).
   - Ziyaret_sebebi: enum kodu (Calisma, Kesif...) Türkçe metne çevrilir.
     ⚠️ DOĞRULA: eski sistemdeki gerçek yazımla (büyük/küçük harf vb.)
     birebir aynı olması gerekiyorsa, aşağıdaki tanılama sorgusuyla
     karşılaştırıp CASE'teki metinleri düzeltin.
   - Arac_plaka: yeni sistemdeki TakipNumarasi snapshot'ı.
   - Kişi+araç birlikte girişi: eski sistemde TEK satırdı, yeni sistemde
     İKİ ayrı satırdır (biri KisiId, biri AracId dolu). Bu sorgu satırları
     birleştirmez — her yeni satır ayrı bir eski-şema satırı olarak döner
     (kişi satırında Arac_plaka NULL, araç satırında Tckn/Ad/Soyad NULL).
   - Eski tabloda karşılığı olmayan yeni alanlar (Tekne, Durum,
     BeklenenBitisZamani, Kaydeden) en altta "EK KOLONLAR" olarak
     yorum satırında bırakıldı; gerekirse açın.

   ⚠️ Eski tablonun kolon listesi aktarım script'inde kullanılan kolonlardan
   alındı. Eski tabloda başka kolonlar da varsa (Id, Tekne vb.) önce şu
   sorguyla tam listeyi görün ve gerekirse eşleştirmeye ekleyin:

   SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
   FROM [eflow].INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'CekekTakip'
   ORDER BY ORDINAL_POSITION;

   Eski Ziyaret_sebebi yazımlarını görmek için:
   SELECT DISTINCT Ziyaret_sebebi FROM [eflow].[dbo].[CekekTakip];
   ============================================================================ */

SELECT
    NULLIF(LTRIM(RTRIM(t.KimlikNumarasi)), '')                          AS Tckn,

    -- Ad: son kelime hariç her şey (tek kelimelik isimde tamamı Ad olur)
    CASE WHEN t.KisiId IS NULL OR NULLIF(LTRIM(RTRIM(t.AdSoyad)), '') IS NULL
            THEN NULL
         WHEN CHARINDEX(' ', LTRIM(RTRIM(t.AdSoyad))) = 0
            THEN LTRIM(RTRIM(t.AdSoyad))
         ELSE RTRIM(LEFT(LTRIM(RTRIM(t.AdSoyad)),
                    LEN(LTRIM(RTRIM(t.AdSoyad)))
                    - CHARINDEX(' ', REVERSE(LTRIM(RTRIM(t.AdSoyad))))))
    END                                                                 AS Ad,

    -- Soyad: son kelime
    CASE WHEN t.KisiId IS NULL OR NULLIF(LTRIM(RTRIM(t.AdSoyad)), '') IS NULL
            THEN NULL
         WHEN CHARINDEX(' ', LTRIM(RTRIM(t.AdSoyad))) = 0
            THEN NULL
         ELSE RIGHT(LTRIM(RTRIM(t.AdSoyad)),
                    CHARINDEX(' ', REVERSE(LTRIM(RTRIM(t.AdSoyad)))) - 1)
    END                                                                 AS Soyad,

    t.FirmaAdi                                                          AS Firma_ismi,
    t.Telefon                                                           AS Telefon_numarasi,

    CASE WHEN t.GirisTarihi IS NULL THEN NULL
         ELSE CAST(t.GirisTarihi AS datetime)
              + CAST(ISNULL(t.GirisSaati, '00:00') AS datetime) END     AS GirisTarihi,

    CASE WHEN t.CikisTarihi IS NULL THEN NULL
         ELSE CAST(t.CikisTarihi AS datetime)
              + CAST(ISNULL(t.CikisSaati, '00:00') AS datetime) END     AS CikisTarihi,

    -- ⚠️ DOĞRULA: eski sistemdeki gerçek yazımla karşılaştırın.
    CASE t.ZiyaretSebebi
         WHEN N'Calisma'        THEN N'Çalışma'
         WHEN N'Gorusme'        THEN N'Görüşme'
         WHEN N'Kesif'          THEN N'Keşif'
         WHEN N'Kontrol'        THEN N'Kontrol'
         WHEN N'MalzemeAlma'    THEN N'Malzeme Alma'
         WHEN N'MalzemeBirakma' THEN N'Malzeme Bırakma'
         WHEN N'IskeleKurma'    THEN N'İskele Kurma'
         ELSE t.ZiyaretSebebi END                                       AS Ziyaret_sebebi,

    t.Aciklama                                                          AS Aciklama,
    NULLIF(LTRIM(RTRIM(t.TakipNumarasi)), '')                           AS Arac_plaka

    /* ---- EK KOLONLAR (eski şemada yok, gerekirse açın) ----
    , tk.TekneKodu                                                      AS TekneKodu
    , tk.TekneAdi                                                       AS TekneAdi
    , CASE t.Durum WHEN 0 THEN N'Giriş Yapıldı'
                   WHEN 1 THEN N'Süresi Geçti'
                   WHEN 2 THEN N'Çıkış Yapıldı' END                    AS Durum
    , t.BeklenenBitisZamani                                             AS BeklenenBitisZamani
    , t.Kaydeden                                                        AS Kaydeden
    , CASE WHEN t.KisiId IS NOT NULL THEN N'Kişi' ELSE N'Araç' END      AS KayitTipi
    */
FROM [ViaCekek].[dbo].[CekekTakipleri] t
LEFT JOIN [ViaCekek].[dbo].[Tekneler] tk ON tk.Id = t.TekneId
-- Yalnızca yeni sistemde girilen kayıtlar (eski sistemden aktarılanlar hariç)
-- isteniyorsa aşağıdaki satırı açın:
-- WHERE t.Kaydeden <> N'eski-sistem-aktarim'
ORDER BY t.GirisTarihi DESC, t.GirisSaati DESC;
