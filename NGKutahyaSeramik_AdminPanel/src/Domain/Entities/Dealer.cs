using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 25 — Bayi Yönetimi (ADR-008: tek `Dealer` entity + `Category` alanıyla Bayi/Showroom ayrımı,
/// ayrı bir Showroom ana entity'si yok). Madde 25.1'in veri modeli tablosu — Product/Blog/Proje'nin
/// aksine — hiçbir alanı "(multi-lang)" işaretlemiyor: Name/Address/City/District native, Translation
/// KULLANILMIYOR (projede Translation'ı hiç tüketmeyen ilk CMS modülü). Aynı tabloda görsel/logo,
/// açıklama, çalışma saatleri, sıralama alanı da yok — Madde 26'nın "galeri görselleri/çalışma
/// saatleri/randevu formu... eklenebilir" ifadesi ADR-008'de zaten "otomatik zorunlu değil" olarak
/// işaretlenmişti; bu alanlar bu fazda uygulanmadı (bkz. ADR-008 güncellemesi).
/// Kategorisiz (17) kayıtların gerçekliğini yansıtmak için `Category` nullable (mevcut projede
/// NewsCategoryId/BlogCategoryId'nin nullable-FK deseniyle tutarlı bir "henüz sınıflandırılmamış"
/// temsili — yeni bir "Unclassified" enum üyesi icat edilmedi).
/// </summary>
public class Dealer
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DealerCategory? Category { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? District { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Fax { get; private set; }
    public string? Email { get; private set; }
    public string? WorkingHours { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Region { get; private set; }
    public string? RegionName { get; private set; }
    public bool IsActive { get; private set; }
    public string BrandCodes { get; private set; } = string.Join(',', Enum.GetNames<ProductBrand>());
    public IReadOnlyList<ProductBrand> Brands => ParseBrands(BrandCodes);

    private Dealer()
    {
    }

    public Dealer(
        string name,
        string city,
        DealerCategory? category,
        string? district,
        string? address,
        string? phone,
        string? fax,
        string? email,
        string? workingHours,
        decimal? latitude,
        decimal? longitude,
        string? region,
        string? regionName,
        IReadOnlyCollection<ProductBrand>? brands = null)
    {
        Name = name;
        City = city;
        Category = category;
        District = district;
        Address = address;
        Phone = phone;
        Fax = fax;
        Email = email;
        WorkingHours = workingHours;
        Latitude = latitude;
        Longitude = longitude;
        Region = region;
        RegionName = regionName;
        IsActive = true;
        SetBrands(brands);
    }

    public void UpdateDetails(
        string name,
        string city,
        DealerCategory? category,
        string? district,
        string? address,
        string? phone,
        string? fax,
        string? email,
        string? workingHours,
        decimal? latitude,
        decimal? longitude,
        string? region,
        string? regionName,
        IReadOnlyCollection<ProductBrand>? brands = null)
    {
        Name = name;
        City = city;
        Category = category;
        District = district;
        Address = address;
        Phone = phone;
        Fax = fax;
        Email = email;
        WorkingHours = workingHours;
        Latitude = latitude;
        Longitude = longitude;
        Region = region;
        RegionName = regionName;
        SetBrands(brands);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetBrands(IEnumerable<ProductBrand>? brands)
    {
        var values = (brands ?? Array.Empty<ProductBrand>()).Distinct().OrderBy(value => value).ToArray();
        if (values.Length == 0)
        {
            values = Enum.GetValues<ProductBrand>();
        }

        BrandCodes = string.Join(',', values);
    }

    private static IReadOnlyList<ProductBrand> ParseBrands(string? values)
    {
        var parsed = (values ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => Enum.TryParse<ProductBrand>(value, true, out var brand) ? (ProductBrand?)brand : null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Distinct()
            .ToArray();

        return parsed.Length > 0 ? parsed : Enum.GetValues<ProductBrand>();
    }
}
