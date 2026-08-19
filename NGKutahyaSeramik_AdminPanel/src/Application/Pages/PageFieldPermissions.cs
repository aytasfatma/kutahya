namespace Application.Pages;

/// <summary>
/// Alan-seviyeli RBAC (backlog #23): SEO Editörü artık Sayfa Yönetimi'nde Edit action'ına erişebiliyor
/// (önceden ViewRoles'taydı, hiç düzenleyemiyordu) — ama yalnızca SEO alanlarını (SeoUrl/MetaTitle/
/// MetaDescription) değiştirebilir. `Title` (sayfa içeriği) ve içerik blokları (PageContentBlockController,
/// hâlâ yalnızca Admin+İçerik Editörü) SEO Editörü için her zaman salt-okunurdur.
/// </summary>
public static class PageFieldPermissions
{
    public static readonly IReadOnlyList<string> SeoEditorFields =
        [PageFields.SeoUrl, PageFields.MetaTitle, PageFields.MetaDescription];
}
