using System.ComponentModel.DataAnnotations;

namespace ViaCekek.Web.Models.Common;

public abstract class AuditableEntity
{
    public DateTime KayitTarihi { get; set; }

    [MaxLength(256)]
    public string Kaydeden { get; set; } = string.Empty;

    public DateTime? GuncellemeTarihi { get; set; }

    [MaxLength(256)]
    public string? Guncelleyen { get; set; }
}
