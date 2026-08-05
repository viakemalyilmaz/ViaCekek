using System.ComponentModel.DataAnnotations;
using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// Spesifik bir araca değil, araç türüne bağlı genel kontrol listesi/kural
// tablosu (bkz. CLAUDE.md) — AracId yok, bilerek. Alindi ve
// GecerlilikTarihiKontrolu birer VERİ değil, KONTROL KURALI: giriş
// ekranında bu belge için hangi kontrollerin yapılacağını belirler
// (Alindi=true → alındı mı diye sorulur; GecerlilikTarihiKontrolu=true →
// ayrıca geçerlilik tarihine de bakılır).
public class AracBelge : AuditableEntity
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string BelgeTanimi { get; set; } = string.Empty;

    // Bu belge için "alındı mı" kontrolü yapılsın mı.
    public bool Alindi { get; set; }

    // Bu belge için ayrıca geçerlilik tarihi kontrolü yapılsın mı.
    public bool GecerlilikTarihiKontrolu { get; set; }

    public bool Aktif { get; set; } = true;

    // Bu belgenin kontrolü hangi araç türlerinde yapılır: birbirini dışlamaz,
    // işaretli türler için giriş sırasında bu belge sorgulanır.
    public bool GecerliArac { get; set; }
    public bool GecerliVinc { get; set; }
    public bool GecerliVidanjor { get; set; }
    public bool GecerliKompresor { get; set; }
    public bool GecerliBasincliKap { get; set; }
}
