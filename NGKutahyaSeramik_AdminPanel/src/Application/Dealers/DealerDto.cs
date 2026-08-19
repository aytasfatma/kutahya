using Domain.Enums;

namespace Application.Dealers;

public class DealerDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public DealerCategory? Category { get; init; }
    public string? District { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? Email { get; init; }
    public string? WorkingHours { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Region { get; init; }
    public string? RegionName { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Array.Empty<ProductBrand>();

    public string CategoryLabel => DealerEnumDisplay.GetCategoryLabel(Category);
}
