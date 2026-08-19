using Application.Roles;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>
/// Madde 30 — Kullanıcı/Rol Yönetimi (Admin=Tam, diğer 3 rol=—). Task 17 kapsamı: salt-okunur —
/// dinamik rol CRUD/permission/claim sistemi kasıtlı olarak yok (bkz. IRoleManagementService xmldoc).
/// `ApplicationRoles.All` tek doğruluk kaynağı: DB'de (hipotetik olarak) fazladan/beklenmeyen bir rol
/// bulunsa bile yalnızca bu listedeki 4 rol sistem rolü olarak gösterilir — `_roleManager.Roles`
/// doğrudan enumerate edilmez.
/// </summary>
public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleManagementService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
    {
        var result = new List<RoleDto>();

        foreach (var roleName in ApplicationRoles.All)
        {
            var (active, total) = await GetUserCountsAsync(roleName);
            result.Add(new RoleDto { Name = roleName, ActiveUserCount = active, TotalUserCount = total });
        }

        return result;
    }

    public async Task<RoleDetailDto?> GetByNameAsync(string roleName)
    {
        if (!ApplicationRoles.All.Contains(roleName))
        {
            return null;
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        var users = usersInRole
            .OrderBy(u => u.Email)
            .Select(u => new RoleUserDto { Id = u.Id, Email = u.Email ?? string.Empty, IsActive = u.IsActive })
            .ToList();

        return new RoleDetailDto
        {
            Name = roleName,
            Description = RoleDescriptions.GetValueOrDefault(roleName, string.Empty),
            Users = users,
            ActiveUserCount = users.Count(u => u.IsActive),
            TotalUserCount = users.Count,
            Permissions = PermissionMatrix
                .Select(module => new RolePermissionDto
                {
                    ModuleName = module.ModuleName,
                    AccessLevel = module.AccessByRole.GetValueOrDefault(roleName, RoleAccessLevel.None),
                    Note = module.Note
                })
                .ToList()
        };
    }

    private async Task<(int Active, int Total)> GetUserCountsAsync(string roleName)
    {
        // RoleExistsAsync=false olsa bile (ör. eksik seed) GetUsersInRoleAsync boş liste döner,
        // fırlatmaz — "kontrollü davranış" burada doğal olarak sağlanıyor, ekstra guard gerekmiyor.
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        return (usersInRole.Count(u => u.IsActive), usersInRole.Count);
    }

    /// <summary>Madde 7.2'nin rol-yetki kapsamı cümleleri — doğrudan doküman metni.</summary>
    private static readonly IReadOnlyDictionary<string, string> RoleDescriptions = new Dictionary<string, string>
    {
        [ApplicationRoles.Admin] = "Tüm modüller, kullanıcı yönetimi, sistem ayarları, dil yönetimi.",
        [ApplicationRoles.ContentEditor] = "Sayfa, blog, haber, banner, referans proje, ürün açıklamaları üzerinde düzenleme yetkisi.",
        [ApplicationRoles.SeoEditor] = "Meta alanları, URL yönetimi, redirect, schema, sitemap, robots.txt.",
        [ApplicationRoles.ProductManager] = "Ürün ekleme/düzenleme, Excel import, görsel yükleme, doküman ilişkilendirme."
    };

    private sealed record ModulePermission(string ModuleName, IReadOnlyDictionary<string, RoleAccessLevel> AccessByRole, string? Note = null);

    /// <summary>
    /// Madde 17.2'nin 15 modülünden türetilmiş, gerçek `[Authorize(Roles=...)]`/`ViewRoles`/`EditRoles`
    /// sabitlerinden (Presentation katmanındaki controller'lar) ELLE çıkarılmış statik matris —
    /// dinamik/reflection-tabanlı otomatik keşif KASITLI olarak yapılmadı (Task 17: "yeni bir dinamik
    /// permission sistemi oluşturma" yasağı). RBAC değiştiğinde bu tablo elle güncellenmelidir.
    /// Henüz controller'ı olmayan modüller (Dil/SEO/Excel Import) "Henüz uygulanmadı" ile işaretli —
    /// tahmini bir erişim seviyesi verilmedi.
    /// </summary>
    private static readonly IReadOnlyList<ModulePermission> PermissionMatrix =
    [
        Module("Dashboard", viewOnly: ApplicationRoles.All),
        Module("Sayfa Yönetimi",
            full: [ApplicationRoles.Admin, ApplicationRoles.ContentEditor],
            partialFields: [ApplicationRoles.SeoEditor],
            note: "SEO Editörü yalnızca SeoUrl/MetaTitle/MetaDescription'ı düzenleyebilir (backlog #23) — Title ve içerik blokları salt-okunur, yeni sayfa oluşturamaz."),
        Module("Ürün Yönetimi",
            full: [ApplicationRoles.Admin, ApplicationRoles.ProductManager],
            partialFields: [ApplicationRoles.ContentEditor, ApplicationRoles.SeoEditor],
            note: "Silme yalnızca Admin'e özgü. İçerik Editörü yalnızca Name/ShortDescription/LongDescription, SEO Editörü yalnızca SeoUrl/MetaTitle/MetaDescription düzenleyebilir (backlog #23) — native alanlar salt-okunur, yeni ürün oluşturamazlar."),
        Module("Koleksiyon/Kategori Yönetimi",
            full: [ApplicationRoles.Admin, ApplicationRoles.ProductManager],
            viewOnly: [ApplicationRoles.ContentEditor, ApplicationRoles.SeoEditor],
            note: "Silme yalnızca Admin'e özgü (ayrı [Authorize(Roles=Admin)] Delete action) — Ürün Yöneticisi Create/Edit yapabilir, silemez."),
        Module("Blog Yönetimi", full: [ApplicationRoles.Admin, ApplicationRoles.ContentEditor], viewOnly: [ApplicationRoles.SeoEditor]),
        Module("Bülten Yönetimi", full: [ApplicationRoles.Admin, ApplicationRoles.ContentEditor], viewOnly: [ApplicationRoles.SeoEditor]),
        Module("Banner Yönetimi", full: [ApplicationRoles.Admin, ApplicationRoles.ContentEditor]),
        Module("Referans Projeler", full: [ApplicationRoles.Admin, ApplicationRoles.ContentEditor], viewOnly: [ApplicationRoles.SeoEditor]),
        Module("Katalog/Doküman Yönetimi", full: [ApplicationRoles.Admin, ApplicationRoles.ProductManager], viewOnly: [ApplicationRoles.ContentEditor]),
        Module("Bayi/Showroom Yönetimi", full: [ApplicationRoles.Admin]),
        Module("Form Yönetimi", full: [ApplicationRoles.Admin], viewOnly: [ApplicationRoles.ContentEditor]),
        Module("Dil Yönetimi", full: [ApplicationRoles.Admin], note: "Backlog #3 — Index + güvenli Edit (Code salt okunur, TR devre dışı bırakılamaz, yeni dil ekleme/silme yok) + eksik çeviri raporu (ADR-007: EntityType/EntityId/Dil/FieldName bazında tespit, toplam/dile göre/modüle göre sayaç + filtrelenebilir liste — fallback/başka dilden doldurma YOK, salt tespit ve raporlama)."),
        Module("SEO Yönetimi", full: [ApplicationRoles.Admin, ApplicationRoles.SeoEditor], note: "Backlog #4 — Ürün/Kategori/Koleksiyon/Blog/Haber/Sayfa/Referans Proje'nin SeoUrl/MetaTitle/MetaDescription alanlarını merkezi olarak düzenler (ikinci bir SEO tablosu yok, mevcut Translation alanları). Banner/Bayi kapsam dışı (SEO alanları hiç yok)."),
        Module("Kullanıcı/Rol Yönetimi", full: [ApplicationRoles.Admin])
    ];

    private static ModulePermission Module(
        string moduleName,
        IReadOnlyList<string>? full = null,
        IReadOnlyList<string>? viewOnly = null,
        IReadOnlyList<string>? partialFields = null,
        string? note = null)
    {
        var access = ApplicationRoles.All.ToDictionary(role => role, _ => RoleAccessLevel.None);

        foreach (var role in viewOnly ?? [])
        {
            access[role] = RoleAccessLevel.ViewOnly;
        }

        foreach (var role in partialFields ?? [])
        {
            access[role] = RoleAccessLevel.PartialFields;
        }

        foreach (var role in full ?? [])
        {
            access[role] = RoleAccessLevel.Full;
        }

        return new ModulePermission(moduleName, access, note);
    }
}
