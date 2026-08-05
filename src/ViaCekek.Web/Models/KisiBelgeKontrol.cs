using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Models;

// AracBelgeKontrol ile birebir aynı mantık, kişi tarafı için.
public class KisiBelgeKontrol : AuditableEntity
{
    public int Id { get; set; }

    public int CekekTakipId { get; set; }
    public CekekTakip CekekTakip { get; set; } = null!;

    public int KisiBelgeId { get; set; }
    public KisiBelge KisiBelge { get; set; } = null!;

    public bool AlindiSonucu { get; set; }

    public DateTime? GecerlilikTarihiSonucu { get; set; }
}
