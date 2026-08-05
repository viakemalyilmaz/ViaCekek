using System.ComponentModel.DataAnnotations;

namespace ViaCekek.Web.Models;

public class Tekne
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string TekneKodu { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string TekneAdi { get; set; } = string.Empty;

    public bool Aktif { get; set; } = true;
}
