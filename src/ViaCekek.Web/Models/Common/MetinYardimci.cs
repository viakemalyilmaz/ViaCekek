using System.Globalization;

namespace ViaCekek.Web.Models.Common;

public static class MetinYardimci
{
    private static readonly CultureInfo TrKultur = CultureInfo.GetCultureInfo("tr-TR");

    // Türkçe büyük harf kuralına göre çevirir (örn. "i" -> "İ", "ı" -> "I") —
    // invariant/İngilizce ToUpper() bu ayrımı yanlış yapar.
    public static string? BuyukHarf(string? deger) =>
        string.IsNullOrEmpty(deger) ? deger : deger.ToUpper(TrKultur);
}
