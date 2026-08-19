namespace Infrastructure.Identity;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string ContentEditor = "İçerik Editörü";
    public const string SeoEditor = "SEO Editörü";
    public const string ProductManager = "Ürün Yöneticisi";

    /// <summary>Task 16 — Kullanıcı Yönetimi'nde rol ataması yalnızca bu whitelist'ten kabul edilir;
    /// serbest metin veya yeni rol adı kabul edilmez (Role Management bu task'ın kapsamı dışı).</summary>
    public static readonly IReadOnlyList<string> All = [Admin, ContentEditor, SeoEditor, ProductManager];
}
