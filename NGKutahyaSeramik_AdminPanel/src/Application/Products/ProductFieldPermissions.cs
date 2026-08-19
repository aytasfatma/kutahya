namespace Application.Products;

/// <summary>
/// Alan-seviyeli RBAC (backlog #23): İçerik Editörü ve SEO Editörü artık Ürün Yönetimi'nde Edit
/// action'ına erişebiliyor (önceden ikisi de yalnızca ViewRoles'taydı, hiç düzenleyemiyordu) — ama
/// yalnızca burada listelenen Translation alanlarını değiştirebilirler. Tüm native (Translation
/// olmayan) alanlar (ProductCode/CategoryId/CollectionId/Brand/Status/Size/Unit/... DisplayOrder)
/// bu iki rol için her zaman salt-okunurdur; yalnızca Admin/Ürün Yöneticisi değiştirebilir. Kontrol
/// hem View'da (disabled input) hem Controller'da (POST'ta izinsiz alanlar mevcut DB değeriyle
/// geri yazılır) uygulanır — yalnızca UI'da gizlemek yeterli sayılmaz.
/// </summary>
public static class ProductFieldPermissions
{
    public static readonly IReadOnlyList<string> ContentEditorFields =
        [ProductFields.Name, ProductFields.ShortDescription, ProductFields.LongDescription];

    public static readonly IReadOnlyList<string> SeoEditorFields =
        [ProductFields.SeoUrl, ProductFields.MetaTitle, ProductFields.MetaDescription];
}
