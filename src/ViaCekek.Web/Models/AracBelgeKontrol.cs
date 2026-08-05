using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// Kural tablosu olan AracBelge'nin aksine bu tablo GERÇEK kontrol
// olaylarının kaydıdır (log): bir CekekTakip girişinde kontrol edildikçe
// yeni satır eklenir, üzerine yazılmaz — bu yüzden AracBelge'deki gibi
// bir tekillik (unique) kısıtı yok. Hangi aracın kontrol edildiği
// CekekTakip.AracId üzerinden bilinir, burada tekrar tutulmaz.
public class AracBelgeKontrol : AuditableEntity
{
    public int Id { get; set; }

    public int CekekTakipId { get; set; }
    public CekekTakip CekekTakip { get; set; } = null!;

    // Hangi kural/belge tanımına göre kontrol edildiği
    public int AracBelgeId { get; set; }
    public AracBelge AracBelge { get; set; } = null!;

    // Kontrol anında belge alınmış/mevcut bulundu mu
    public bool AlindiSonucu { get; set; }

    // AracBelge.GecerlilikTarihiKontrolu = true ise, kontrolde görülen gerçek tarih
    public DateTime? GecerlilikTarihiSonucu { get; set; }
}
