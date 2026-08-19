using Domain.Enums;

namespace Domain.Entities;

public class Surface
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? SeoUrl { get; private set; }
    public string? ImagePath { get; private set; }
    public string BrandCodes { get; private set; } = "NgSeramik,NgStone,NgSlim,NgPerforma";
    public IReadOnlyList<ProductBrand> Brands => BrandCodes.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => Enum.TryParse<ProductBrand>(x, out var brand) ? brand : (ProductBrand?)null)
        .Where(x => x.HasValue).Select(x => x!.Value).ToList();
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Surface() { }

    public Surface(string name, int displayOrder)
    {
        Name = name.Trim();
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void Update(string name, int displayOrder)
    {
        Name = name.Trim();
        DisplayOrder = displayOrder;
    }

    public void SetContent(string? imagePath, IEnumerable<ProductBrand> brands)
    {
        ImagePath = imagePath;
        var values = brands.Distinct().OrderBy(x => x).ToArray();
        if (values.Length == 0) throw new ArgumentException("En az bir marka seçilmelidir.", nameof(brands));
        BrandCodes = string.Join(',', values);
    }
    public void SetSeoUrl(string? seoUrl) => SeoUrl = string.IsNullOrWhiteSpace(seoUrl) ? null : seoUrl.Trim();

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
