using System.Text;

namespace Application.Seo;

/// <summary>Madde 27.2'nin slug-bazlı URL yapısı için basit, bağımlılıksız normalizasyon: küçük harfe
/// çevirir (Türkçe İ/I/ı özel durumları dahil), boşluk/alt çizgiyi tireye çevirir, yalnızca a-z/0-9/tire
/// bırakır, ardışık tireleri tekilleştirir, baş/son tireyi kırpar. SeoUrl'ler DB'de olduğu gibi (kullanıcının
/// girdiği hâliyle) saklanır — normalizasyon yalnızca duplicate KARŞILAŞTIRMASI ve kaydetme öncesi
/// öneri/otomatik-düzeltme için kullanılır.</summary>
public static class SeoUrlNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = value.Trim()
            .Replace('İ', 'i').Replace('I', 'ı').Replace('ı', 'i')
            .Replace('Ğ', 'g').Replace('ğ', 'g')
            .Replace('Ü', 'u').Replace('ü', 'u')
            .Replace('Ş', 's').Replace('ş', 's')
            .Replace('Ö', 'o').Replace('ö', 'o')
            .Replace('Ç', 'c').Replace('ç', 'c')
            .ToLowerInvariant();

        var builder = new StringBuilder(text.Length);
        var lastWasHyphen = false;

        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch) && ch < 128)
            {
                builder.Append(ch);
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                lastWasHyphen = true;
            }
        }

        var result = builder.ToString().Trim('-');
        return result;
    }
}
