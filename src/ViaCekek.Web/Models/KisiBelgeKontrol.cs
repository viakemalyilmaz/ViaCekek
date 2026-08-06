using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// Artık bir loglama tablosu değil, kişiye bağlı GÜNCEL belge durumu
// (bir kişi × bir belge kuralı = tek satır, upsert edilir). CekekTakipId
// opsiyonel: yalnızca bir Çekek Takip girişinde teyit/güncelleme
// yapıldıysa doldurulur; kayıt anında (henüz hiçbir giriş yokken) da
// girilebilir.
public class KisiBelgeKontrol : AuditableEntity
{
    public int Id { get; set; }

    public int KisiId { get; set; }
    public Kisi Kisi { get; set; } = null!;

    public int KisiBelgeId { get; set; }
    public KisiBelge KisiBelge { get; set; } = null!;

    public int? CekekTakipId { get; set; }
    public CekekTakip? CekekTakip { get; set; }

    public bool AlindiSonucu { get; set; }

    public DateTime? GecerlilikTarihiSonucu { get; set; }
}
