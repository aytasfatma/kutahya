using Domain.Enums;

namespace Application.Surfaces;

public class SurfaceListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Array.Empty<ProductBrand>();
    public bool IsActive { get; init; }
}
