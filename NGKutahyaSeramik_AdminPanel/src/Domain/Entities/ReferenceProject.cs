using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 23 — Referans Projeler. Product ile ilişkisi many-to-many'dir (Madde 23.1 RelatedProducts /
/// Madde 36.2) ve tamamen opsiyoneldir. Müşteri notu gereği (Madde 23) şu an içerik/arşiv olmadan da
/// oluşturulabilir olmalıdır — bu yüzden Location/Architect/Year opsiyoneldir (doküman Zorunluluk
/// sütunu içermiyor).
/// </summary>
public class ReferenceProject
{
    public int Id { get; private set; }
    public string? Location { get; private set; }
    public ReferenceProjectRegion Region { get; private set; }
    public ProductBrand Brand { get; private set; }
    public ReferenceProjectType ProjectType { get; private set; }
    public string? Architect { get; private set; }
    public int? Year { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private ReferenceProject()
    {
    }

    public ReferenceProject(string? location, ReferenceProjectRegion region, ProductBrand brand, ReferenceProjectType projectType, string? architect, int? year, int displayOrder)
    {
        Location = location;
        Region = region;
        Brand = brand;
        ProjectType = projectType;
        Architect = architect;
        Year = year;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateDetails(string? location, ReferenceProjectRegion region, ProductBrand brand, ReferenceProjectType projectType, string? architect, int? year, int displayOrder)
    {
        Location = location;
        Region = region;
        Brand = brand;
        ProjectType = projectType;
        Architect = architect;
        Year = year;
        DisplayOrder = displayOrder;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
