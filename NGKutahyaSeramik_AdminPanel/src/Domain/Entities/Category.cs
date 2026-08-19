using Domain.Enums;

namespace Domain.Entities;

public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? SeoUrl { get; private set; }
    public int? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public string? ImagePath { get; private set; }
    public string BrandCodes { get; private set; } = "NgSeramik,NgStone,NgSlim";
    public IReadOnlyList<ProductBrand> Brands => BrandCodes.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => Enum.TryParse<ProductBrand>(x, out var brand) ? brand : (ProductBrand?)null)
        .Where(x => x.HasValue).Select(x => x!.Value).ToList();
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Category()
    {
    }

    public Category(int? parentCategoryId, string? imagePath, int displayOrder)
    {
        ParentCategoryId = parentCategoryId;
        ImagePath = imagePath;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateDetails(int? parentCategoryId, string? imagePath, int displayOrder)
    {
        ParentCategoryId = parentCategoryId;
        ImagePath = imagePath;
        DisplayOrder = displayOrder;
    }

    public void SetIdentity(string name, string? seoUrl) { Name = name.Trim(); SeoUrl = string.IsNullOrWhiteSpace(seoUrl) ? null : seoUrl.Trim(); }

    public void SetBrands(IEnumerable<ProductBrand> brands)
    {
        var values = brands.Distinct().OrderBy(x => x).ToArray();
        if (values.Length == 0) throw new ArgumentException("En az bir marka seçilmelidir.", nameof(brands));
        BrandCodes = string.Join(',', values);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
