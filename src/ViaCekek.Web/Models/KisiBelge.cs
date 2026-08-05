using System.ComponentModel.DataAnnotations;
using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// AracBelge ile birebir aynı mantık: bir kişiye değil, kural/tanıma bağlı
// genel kontrol listesi. Alindi ve GecerlilikTarihiKontrolu birer VERİ
// değil, KONTROL KURALI (bkz. AracBelge).
public class KisiBelge : AuditableEntity
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string BelgeTanimi { get; set; } = string.Empty;

    public bool Alindi { get; set; }

    public bool GecerlilikTarihiKontrolu { get; set; }

    public bool Aktif { get; set; } = true;

    // Bu belgenin kontrolü hangi ziyaret sebeplerinde yapılır: birbirini
    // dışlamaz, işaretli sebepler için giriş sırasında bu belge sorgulanır.
    public bool GecerliCalisma { get; set; }
    public bool GecerliGorusme { get; set; }
    public bool GecerliKesif { get; set; }
    public bool GecerliKontrol { get; set; }
    public bool GecerliMalzemeAlma { get; set; }
    public bool GecerliMalzemeBirakma { get; set; }
    public bool GecerliIskeleKurma { get; set; }
}
