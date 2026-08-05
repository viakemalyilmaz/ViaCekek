using System.ComponentModel.DataAnnotations;
using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

public class Arac : AuditableEntity
{
    public int Id { get; set; }

    // Plaka veya benzeri tekillik bilgisi (vinç/vidanjör/kompresör/basınçlı kap için plaka olmayabilir)
    [Required, MaxLength(50)]
    public string TakipNumarasi { get; set; } = string.Empty;

    public AracTuru AracTuru { get; set; }

    // Serbest metin: UI'da önceden girilmiş Araclar.FirmaAdi değerleri üzerinden
    // autocomplete ile önerilecek, Kisi.FirmaAdi ile aynı yaklaşım (ayrı Firma tablosu yok).
    [MaxLength(200)]
    public string? FirmaAdi { get; set; }

    public bool Aktif { get; set; } = true;

    [MaxLength(500)]
    public string? YasaklanmaSebebi { get; set; }
}
