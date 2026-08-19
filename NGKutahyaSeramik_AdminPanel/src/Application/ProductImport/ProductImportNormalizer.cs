using System.Globalization;
using System.Text.RegularExpressions;
using Domain.Enums;

namespace Application.ProductImport;

public partial class ProductImportNormalizer
{
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public string? NullIfBlankOrDash(string? value)
    {
        var normalized = NormalizeWhitespace(value);
        return normalized is null || normalized == "-" ? null : normalized;
    }

    public string? NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return SpaceRegex().Replace(value.Trim(), " ");
    }

    public string? NormalizeSize(string? value) =>
        NullIfBlankOrDash(value)?.Replace('*', 'x');

    public string? NormalizeUnit(string? value)
    {
        var normalized = UpperKey(value)?.Replace("²", "2");
        return normalized switch
        {
            null or "-" => null,
            "M2" => "M2",
            "ADT" => "ADT",
            _ => null
        };
    }

    public ProductStatus? NormalizeStatus(string? value) =>
        UpperKey(value) switch
        {
            "AKTİF" or "AKTIF" or "ACTIVE" => ProductStatus.Active,
            "PASİF" or "PASIF" or "INACTIVE" => ProductStatus.Inactive,
            "DEVAM" => ProductStatus.Active,
            "INPROGRESS" or "IN PROGRESS" => ProductStatus.InProgress,
            "İPTAL" or "IPTAL" or "CANCELLED" or "CANCELED" => ProductStatus.Cancelled,
            _ => null
        };

    public ProductBrand? NormalizeBrand(string? value) =>
        UpperKey(value)?.Replace(" ", "") switch
        {
            "NGSERAMİK" or "NGSERAMIK" => ProductBrand.NgSeramik,
            "NGSTONE" => ProductBrand.NgStone,
            "NGSLIM" => ProductBrand.NgSlim,
            "NGPERFORMA" => ProductBrand.NgPerforma,
            _ => null
        };

    public bool? NormalizeBoolean(string? value) =>
        UpperKey(value) switch
        {
            null or "-" => null,
            "EVET" or "VAR" or "TRUE" or "1" => true,
            "HAYIR" or "YOK" or "FALSE" or "0" => false,
            _ => null
        };

    public bool TryParseBoolean(string? value, out bool? result)
    {
        var normalized = UpperKey(value);
        if (normalized is null or "-")
        {
            result = null;
            return true;
        }

        result = NormalizeBoolean(value);
        return result.HasValue;
    }

    public decimal? NormalizeDecimal(string? value)
    {
        var cleaned = NullIfBlankOrDash(value);
        if (cleaned is null)
        {
            return null;
        }

        cleaned = cleaned
            .Replace("mm", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("m²", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("m2", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim()
            .Replace(',', '.');

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public int? NormalizeInt(string? value)
    {
        var cleaned = NullIfBlankOrDash(value);
        return cleaned is not null && int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public decimal? NormalizePEI(string? value)
    {
        var cleaned = NullIfBlankOrDash(value);
        if (cleaned is null)
        {
            return null;
        }

        cleaned = cleaned.Replace("PEI", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        return decimal.TryParse(cleaned.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public string? NormalizeVValue(string? value)
    {
        var cleaned = UpperKey(value);
        return cleaned is null or "-" ? null : cleaned.Replace(" ", string.Empty);
    }

    public string? NormalizeRValue(string? value)
    {
        var cleaned = UpperKey(value);
        if (cleaned is null or "-")
        {
            return null;
        }

        cleaned = cleaned
            .Replace(" ", string.Empty)
            .Replace("–", "-")
            .Replace("—", "-");

        return cleaned switch
        {
            "R9" or "R10" or "R11" or "R12" or "R13" or "R11-R12" => cleaned,
            _ => null
        };
    }

    public (bool? AntiSlip, string? RValue, bool Recognized) NormalizeAntiSlip(string? value)
    {
        var cleaned = UpperKey(value);
        if (cleaned is null or "-")
        {
            return (null, null, true);
        }

        if (cleaned is "HAYIR" or "YOK" or "FALSE" or "0")
        {
            return (false, null, true);
        }

        if (cleaned is "EVET" or "VAR" or "TRUE" or "1")
        {
            return (true, null, true);
        }

        var compact = cleaned.Replace(" ", string.Empty);
        if (compact.Contains("R11-R12", StringComparison.Ordinal) || compact.Contains("R11-R12ARALIĞINAUYGUN", StringComparison.Ordinal))
        {
            return (true, "R11-R12", true);
        }

        foreach (var r in new[] { "R13", "R12", "R11", "R10", "R9" })
        {
            if (compact.Contains(r, StringComparison.Ordinal))
            {
                return (true, r, true);
            }
        }

        return (null, null, false);
    }

    public string? NormalizeApplicationArea(string? value) =>
        NormalizeTokenList(value, ("YER", "Yer"), ("DUVAR", "Duvar"));

    public string? NormalizeUsageArea(string? value) =>
        NormalizeTokenList(value, ("BANYO", "Banyo"), ("MUTFAK", "Mutfak"));

    public bool ReferenceEquals(string? left, string? right)
    {
        var l = UpperKey(left);
        var r = UpperKey(right);
        return l is not null && r is not null && l == r;
    }

    private string? NormalizeTokenList(string? value, params (string Key, string Label)[] knownTokens)
    {
        var cleaned = UpperKey(value);
        if (cleaned is null or "-")
        {
            return null;
        }

        var selected = knownTokens
            .Where(token => cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(token.Key))
            .Select(token => token.Label)
            .ToList();

        return selected.Count == 0 ? NullIfBlankOrDash(value) : string.Join(", ", selected);
    }

    private string? UpperKey(string? value)
    {
        var normalized = NullIfBlankOrDash(value);
        return normalized?.ToUpper(TrCulture);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
}
