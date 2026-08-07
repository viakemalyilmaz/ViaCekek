using System.ComponentModel.DataAnnotations;
using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// Hem kişi hem araç giriş/çıkışları burada tutulur: bir satırda ya KisiId
// ya da AracId dolu olur (ikisi birden değil).
public class CekekTakip : AuditableEntity
{
    public int Id { get; set; }

    public int? KisiId { get; set; }
    public Kisi? Kisi { get; set; }

    public int? AracId { get; set; }
    public Arac? Arac { get; set; }

    // Giriş anındaki anlık görüntü (snapshot): Kisi/Arac kaydı sonradan
    // değişse bile o günkü kayıt bozulmaz.
    [MaxLength(20)]
    public string? KimlikNumarasi { get; set; }

    [MaxLength(50)]
    public string? TakipNumarasi { get; set; }

    [MaxLength(200)]
    public string? AdSoyad { get; set; }

    [MaxLength(200)]
    public string? FirmaAdi { get; set; }

    [MaxLength(15)]
    public string? Telefon { get; set; }

    public int? TekneId { get; set; }
    public Tekne? Tekne { get; set; }

    public DateOnly? GirisTarihi { get; set; }
    public TimeOnly? GirisSaati { get; set; }
    public DateOnly? CikisTarihi { get; set; }
    public TimeOnly? CikisSaati { get; set; }

    public ZiyaretSebebi ZiyaretSebebi { get; set; }

    public CekekTakipDurumu Durum { get; set; } = CekekTakipDurumu.GirisYapildi;

    // Ziyaret Sebebi'ne göre içeride kalma süresinin dolacağı an — süresiz
    // sebeplerde (Çalışma/Görüşme/İskele Kurma) null kalır. "+15 dk" bu
    // alanı ileri alır; board açıldığında süresi geçmiş ama hâlâ
    // GirisYapildi olan kayıtlar ZamanAsimi'ye çevrilir.
    public DateTime? BeklenenBitisZamani { get; set; }

    [MaxLength(1000)]
    public string? Aciklama { get; set; }
}
