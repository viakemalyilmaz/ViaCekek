using System.ComponentModel.DataAnnotations;
using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

public class Kisi : AuditableEntity
{
    public int Id { get; set; }

    // TC Kimlik No veya yabancı ziyaretçiler için pasaport no
    [Required, MaxLength(20)]
    public string KimlikNumarasi { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string AdSoyad { get; set; } = string.Empty;

    // Serbest metin: UI'da önceden girilmiş Kisiler.FirmaAdi değerleri üzerinden
    // autocomplete ile önerilecek, ayrı bir Firma tablosu kullanılmıyor (hız önceliği).
    [MaxLength(200)]
    public string? FirmaAdi { get; set; }

    // Yalnızca rakam, boşluksuz; yurt dışı numaraları da desteklemek için
    // TR'ye özgü format zorunlu kılınmadı (bkz. CLAUDE.md).
    [MaxLength(15)]
    [RegularExpression(@"^[0-9]{7,15}$", ErrorMessage = "Telefon numarası yalnızca rakamlardan oluşmalı (7-15 hane).")]
    public string? Telefon { get; set; }

    public bool Aktif { get; set; } = true;

    [MaxLength(500)]
    public string? YasaklanmaSebebi { get; set; }

    // Ziyaretçi niteliği: birbirini dışlamaz, aynı kişi birden fazlası olabilir.
    public bool TekneSahibi { get; set; }
    public bool Kaptan { get; set; }
    public bool TeknePersoneli { get; set; }

    // KVKK: yalnızca güncel durum tutulur, değişiklik geçmişi
    // Guncelleyen/GuncellemeTarihi audit alanlarından izlenir.
    public bool KvkkOnayFormuAlindi { get; set; }
    public KvkkOnayDurumu KvkkOnayDurumu { get; set; } = KvkkOnayDurumu.Bilinmiyor;
    public DateTime? KvkkOnayTarihi { get; set; }
}
