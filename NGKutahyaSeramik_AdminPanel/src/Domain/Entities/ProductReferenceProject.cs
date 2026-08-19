namespace Domain.Entities;

/// <summary>
/// Ürün-proje ilişki tablosu (Madde 23.1 RelatedProducts / Madde 36.2 — dokümanda "ProductProjects"
/// olarak anılıyor, burada ReferenceProject adlandırmasıyla tutarlılık için ProductReferenceProject
/// kullanıldı). Cascade FK her iki yönde de — ürün veya proje silinirse yalnızca ilişki satırı gider,
/// diğer taraf etkilenmez (Document/ProductDocument ile aynı desen, Madde 36.1/36.2).
/// </summary>
public class ProductReferenceProject
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int ReferenceProjectId { get; private set; }
    public ReferenceProject ReferenceProject { get; private set; } = null!;

    private ProductReferenceProject()
    {
    }

    public ProductReferenceProject(int productId, int referenceProjectId)
    {
        ProductId = productId;
        ReferenceProjectId = referenceProjectId;
    }
}
