using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// Artık bir loglama tablosu değil, araca bağlı GÜNCEL belge durumu
// (bir araç × bir belge kuralı = tek satır, upsert edilir). CekekTakipId
// opsiyonel: yalnızca bir Çekek Takip girişinde teyit/güncelleme
// yapıldıysa doldurulur; kayıt anında (henüz hiçbir giriş yokken) da
// girilebilir. KisiBelgeKontrol ile birebir aynı mantık (bkz. CLAUDE.md).
public class AracBelgeKontrol : AuditableEntity
{
    public int Id { get; set; }

    public int AracId { get; set; }
    public Arac Arac { get; set; } = null!;

    public int AracBelgeId { get; set; }
    public AracBelge AracBelge { get; set; } = null!;

    public int? CekekTakipId { get; set; }
    public CekekTakip? CekekTakip { get; set; }

    public bool AlindiSonucu { get; set; }

    public DateTime? GecerlilikTarihiSonucu { get; set; }
}
