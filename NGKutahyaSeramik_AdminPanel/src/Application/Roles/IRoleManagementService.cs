namespace Application.Roles;

/// <summary>
/// Madde 30 — Kullanıcı/Rol Yönetimi (Madde 17.2 modül #14, User Management ile birleşik tek satır).
/// Task 17 analiz kararı: doküman dinamik rol CRUD'a dair hiçbir kanıt vermiyor (Madde 7.2 dört rolü
/// kapalı bir liste olarak tanımlıyor); mevcut RBAC tamamen derleme-zamanı `[Authorize(Roles=...)]`
/// sabitlerine dayandığından yeni bir rol otomatik erişim kazanamaz — bu yüzden Role Management salt
/// bir görüntüleme ekranı, hiçbir state-changing action yok (rol oluşturma/silme/yeniden adlandırma/
/// kullanıcıya rol atama bu modülün konusu değil, ikincisi User Management'ta zaten var).
///
/// `IUserManagementService` ile aynı katman sınırı sebebiyle interface — implementasyon
/// `RoleManager&lt;IdentityRole&gt;`/`UserManager&lt;ApplicationUser&gt;`'a (Infrastructure.Identity)
/// bağımlı olmak zorunda, Application projesi Infrastructure'a referans veremiyor (bkz. ADR-016).
/// </summary>
public interface IRoleManagementService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync();

    Task<RoleDetailDto?> GetByNameAsync(string roleName);
}
