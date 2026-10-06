/* ============================================================================
   YENİ SİSTEM (ViaCekek.Kisiler + KisiBelgeKontrolleri) -> ESKİ SİSTEM
   (eflow.CekekKisiKayit) ŞEMASINDA SONUÇ DÖNDÜREN SORGU
   ============================================================================
   Hazırlanma tarihi: 2026-09-30

   eski-sistem-veri-aktarimi.sql > ADIM 2 (Kişiler) ve ADIM 4 (Kişi Belge
   Kontrolleri) adımlarının TERSİ: yeni sistemdeki kişileri, eski
   CekekKisiKayit tablosunun kolon adları/biçimiyle, belgeler yine satır
   içi (inline) kolonlar olacak şekilde döndürür. Yalnızca SELECT — hiçbir
   şey değiştirmez.

   DÖNÜŞÜM KURALLARI:
   - Belgeler: yeni sistemde her belge KisiBelgeKontrolleri'nde ayrı satır;
     burada KisiBelgeleri.BelgeTanimi üzerinden (aktarımdaki birebir isim
     eşleşmesiyle) kişi başına tek satıra pivot edilir. Belge "Alındı" ise
     belge kolonuna N'Alındı' yazılır, değilse / kayıt yoksa NULL.
     ⚠️ DOĞRULA: eski sistemde belge kolonlarında "alındı" anlamına gelen
     değer farklıysa (örn. 'E', 'Evet', dosya adı) aşağıdaki
     @AlindiDegeri değişkenini değiştirin. Eski değerleri görmek için:
       SELECT TOP 20 sorumlulukYazisi, sgkHizmetDokumu, isgEgitimBelgesi,
              kvkkOnayFormu
       FROM [eflow].[dbo].[CekekKisiKayit]
       WHERE sorumlulukYazisi IS NOT NULL OR kvkkOnayFormu IS NOT NULL;
   - kvkkOnayFormu: belge tablosunda değil Kisiler.KvkkOnayFormuAlindi'de
     tutuluyor (aktarımda da öyleydi); kvkkOnayFormuTarih <- KvkkOnayTarihi.
   - ZiyaretciTipi: yeni sistemde üç bağımsız checkbox (TekneSahibi, Kaptan,
     TeknePersoneli) -> virgülle birleştirilmiş tek metin. Hiçbiri işaretli
     değilse NULL.
   - kvkkOnay: KvkkOnayDurumu enum'u -> Türkçe metin (Bilinmiyor -> NULL).
     ⚠️ DOĞRULA: ZiyaretciTipi ve kvkkOnay metinlerini eski sistemdeki
     gerçek yazımla karşılaştırın:
       SELECT DISTINCT ZiyaretciTipi FROM [eflow].[dbo].[CekekKisiKayit];
       SELECT DISTINCT kvkkOnay      FROM [eflow].[dbo].[CekekKisiKayit];
   - isAktif / bansebebi / kayittarihi / guncellemetarihi: doğrudan karşılık.
   - Eski tabloda karşılığı olmayan yeni alanlar (Kaydeden, Guncelleyen)
     en altta "EK KOLONLAR" olarak yorum satırında bırakıldı.

   ⚠️ Eski tablonun kolon listesi aktarım script'inde kullanılan kolonlardan
   alındı. Eski tabloda başka kolonlar da varsa tam listeyi görün:
     SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
     FROM [eflow].INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_NAME = 'CekekKisiKayit'
     ORDER BY ORDINAL_POSITION;
   ============================================================================ */

DECLARE @AlindiDegeri nvarchar(50) = N'Alındı';  -- ⚠️ DOĞRULA (bkz. üstteki not)

WITH Belgeler AS (
    -- Kişi başına tek satır: her belge için Alındı durumu + geçerlilik tarihi
    SELECT
        kbk.KisiId,
        MAX(CASE WHEN b.BelgeTanimi = N'İskele Kurma'                   AND kbk.AlindiSonucu = 1 THEN 1 END) AS IskeleKurma,
        MAX(CASE WHEN b.BelgeTanimi = N'İskele Kurma'                   THEN kbk.GecerlilikTarihiSonucu END) AS IskeleKurmaTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Giriş Talimatnamesi'            AND kbk.AlindiSonucu = 1 THEN 1 END) AS GirisTalimatname,
        MAX(CASE WHEN b.BelgeTanimi = N'Sorumluluk Yazısı'              AND kbk.AlindiSonucu = 1 THEN 1 END) AS SorumlulukYazisi,
        MAX(CASE WHEN b.BelgeTanimi = N'Sorumluluk Yazısı'              THEN kbk.GecerlilikTarihiSonucu END) AS SorumlulukYazisiTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Sgk Hizmet Dökümü'              AND kbk.AlindiSonucu = 1 THEN 1 END) AS SgkHizmetDokumu,
        MAX(CASE WHEN b.BelgeTanimi = N'Sgk Hizmet Dökümü'              THEN kbk.GecerlilikTarihiSonucu END) AS SgkHizmetDokumuTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'İsg Eğitim Belgesi'             AND kbk.AlindiSonucu = 1 THEN 1 END) AS IsgEgitimBelgesi,
        MAX(CASE WHEN b.BelgeTanimi = N'İsg Eğitim Belgesi'             THEN kbk.GecerlilikTarihiSonucu END) AS IsgEgitimBelgesiTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Sağlık Raporu'                  AND kbk.AlindiSonucu = 1 THEN 1 END) AS SaglikRaporu,
        MAX(CASE WHEN b.BelgeTanimi = N'Sağlık Raporu'                  THEN kbk.GecerlilikTarihiSonucu END) AS SaglikRaporuTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Görevlendirme Yazısı'           AND kbk.AlindiSonucu = 1 THEN 1 END) AS GorevlendirmeYazisi,
        MAX(CASE WHEN b.BelgeTanimi = N'Görevlendirme Yazısı'           THEN kbk.GecerlilikTarihiSonucu END) AS GorevlendirmeYazisiTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Mesleki Yeterlilik Sertifikası' AND kbk.AlindiSonucu = 1 THEN 1 END) AS MeslekiYeterlilik,
        MAX(CASE WHEN b.BelgeTanimi = N'Mesleki Yeterlilik Sertifikası' THEN kbk.GecerlilikTarihiSonucu END) AS MeslekiYeterlilikTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Yüksekte Çalışma Belgesi'       AND kbk.AlindiSonucu = 1 THEN 1 END) AS YuksekteCalisma,
        MAX(CASE WHEN b.BelgeTanimi = N'Yüksekte Çalışma Belgesi'       THEN kbk.GecerlilikTarihiSonucu END) AS YuksekteCalismaTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Adli Sicil Kaydı'               AND kbk.AlindiSonucu = 1 THEN 1 END) AS AdliSicil,
        MAX(CASE WHEN b.BelgeTanimi = N'Adli Sicil Kaydı'               THEN kbk.GecerlilikTarihiSonucu END) AS AdliSicilTarih,
        MAX(CASE WHEN b.BelgeTanimi = N'Kkd Zimmet Formu'               AND kbk.AlindiSonucu = 1 THEN 1 END) AS KkdZimmet,
        MAX(CASE WHEN b.BelgeTanimi = N'Kkd Zimmet Formu'               THEN kbk.GecerlilikTarihiSonucu END) AS KkdZimmetTarih
    FROM [ViaCekek].[dbo].[KisiBelgeKontrolleri] kbk
    JOIN [ViaCekek].[dbo].[KisiBelgeleri] b ON b.Id = kbk.KisiBelgeId
    GROUP BY kbk.KisiId
)
SELECT
    k.KimlikNumarasi                                                    AS tckn,
    k.AdSoyad                                                           AS adSoyad,
    k.FirmaAdi                                                          AS firmaismi,
    k.Telefon                                                           AS gsm,
    k.Aktif                                                             AS isAktif,
    k.YasaklanmaSebebi                                                  AS bansebebi,

    -- ⚠️ DOĞRULA: eski yazımla karşılaştırın.
    NULLIF(STUFF(
          CASE WHEN k.TekneSahibi    = 1 THEN N', Tekne Sahibi'    ELSE N'' END
        + CASE WHEN k.Kaptan         = 1 THEN N', Kaptan'          ELSE N'' END
        + CASE WHEN k.TeknePersoneli = 1 THEN N', Tekne Personeli' ELSE N'' END
        , 1, 2, N''), N'')                                              AS ZiyaretciTipi,

    -- ⚠️ DOĞRULA: eski yazımla karşılaştırın.
    CASE k.KvkkOnayDurumu
         WHEN N'OnayVerildi'   THEN N'Onay Verildi'
         WHEN N'OnayVerilmedi' THEN N'Onay Verilmedi'
         ELSE NULL END                                                  AS kvkkOnay,
    CASE WHEN k.KvkkOnayFormuAlindi = 1 THEN @AlindiDegeri END          AS kvkkOnayFormu,
    k.KvkkOnayTarihi                                                    AS kvkkOnayFormuTarih,

    CASE WHEN bl.IskeleKurma         = 1 THEN @AlindiDegeri END         AS iskeleKurmaBelgesi,
    bl.IskeleKurmaTarih                                                 AS iskeleKurmaBelgesiTarih,
    CASE WHEN bl.GirisTalimatname    = 1 THEN @AlindiDegeri END         AS GirisTalimatname,
    CASE WHEN bl.SorumlulukYazisi    = 1 THEN @AlindiDegeri END         AS sorumlulukYazisi,
    bl.SorumlulukYazisiTarih                                            AS sorumlulukYazisiTarih,
    CASE WHEN bl.SgkHizmetDokumu     = 1 THEN @AlindiDegeri END         AS sgkHizmetDokumu,
    bl.SgkHizmetDokumuTarih                                             AS sgkHizmetDokumuTarih,
    CASE WHEN bl.IsgEgitimBelgesi    = 1 THEN @AlindiDegeri END         AS isgEgitimBelgesi,
    bl.IsgEgitimBelgesiTarih                                            AS isgEgitimBelgesiTarih,
    CASE WHEN bl.SaglikRaporu        = 1 THEN @AlindiDegeri END         AS saglikRaporu,
    bl.SaglikRaporuTarih                                                AS saglikRaporuTarih,
    CASE WHEN bl.GorevlendirmeYazisi = 1 THEN @AlindiDegeri END         AS gorevlendirmeYazisi,
    bl.GorevlendirmeYazisiTarih                                         AS gorevlendirmeYazisiTarih,
    CASE WHEN bl.MeslekiYeterlilik   = 1 THEN @AlindiDegeri END         AS meslekiYeterlilikSertifikasi,
    bl.MeslekiYeterlilikTarih                                           AS meslekiYeterlilikSertifikasiTarih,
    CASE WHEN bl.YuksekteCalisma     = 1 THEN @AlindiDegeri END         AS yuksekteCalismaBelgesi,
    bl.YuksekteCalismaTarih                                             AS yuksekteCalismaBelgesiTarih,
    CASE WHEN bl.AdliSicil           = 1 THEN @AlindiDegeri END         AS adliSicilKaydi,
    bl.AdliSicilTarih                                                   AS adliSicilKaydiTarih,
    CASE WHEN bl.KkdZimmet           = 1 THEN @AlindiDegeri END         AS kkdZimmetFormu,
    bl.KkdZimmetTarih                                                   AS kkdZimmetFormuTarih,

    k.KayitTarihi                                                       AS kayittarihi,
    k.GuncellemeTarihi                                                  AS guncellemetarihi

    /* ---- EK KOLONLAR (eski şemada yok, gerekirse açın) ----
    , k.Kaydeden                                                        AS Kaydeden
    , k.Guncelleyen                                                     AS Guncelleyen
    */
FROM [ViaCekek].[dbo].[Kisiler] k
LEFT JOIN Belgeler bl ON bl.KisiId = k.Id
-- Yalnızca yeni sistemde girilen kişiler (eski sistemden aktarılanlar hariç)
-- isteniyorsa aşağıdaki satırı açın:
-- WHERE k.Kaydeden <> N'eski-sistem-aktarim'
ORDER BY k.AdSoyad;
