namespace Application.Roles;

/// <summary>Madde 30 + gerçek `[Authorize(Roles=...)]` sabitlerinden elle türetilmiş, salt-okunur
/// erişim seviyesi. Yeni bir dinamik permission sistemi DEĞİL — bkz. RoleManagementService'teki
/// statik matris (Task 17 kapsamı, kasıtlı olarak dinamik/reflection-tabanlı değil).</summary>
public enum RoleAccessLevel
{
    None,
    ViewOnly,
    /// <summary>Backlog #23 — alan-seviyeli RBAC: role, ilgili modülün YALNIZCA bazı alanlarını
    /// düzenleyebilir (ör. SEO Editörü'nün SeoUrl/MetaTitle/MetaDescription'ı). Hangi alanların
    /// izinli olduğu <see cref="RolePermissionDto.Note"/>'ta belirtilir.</summary>
    PartialFields,
    Full,
    NotImplemented
}

public class RolePermissionDto
{
    public string ModuleName { get; init; } = string.Empty;
    public RoleAccessLevel AccessLevel { get; init; }

    /// <summary>Yalnızca Ürün/Koleksiyon/Kategori Yönetimi'nde kullanılıyor — EditRoles'a sahip
    /// olsa da Silme action'ı ayrıca yalnızca Admin'e kilitli (koddaki gerçek istisna, icat değil).</summary>
    public string? Note { get; init; }
}
