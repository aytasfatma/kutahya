using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>Madde 30 (Kullanıcı Yönetimi, Admin-only). IsActive — Identity'nin Lockout mekanizmasından
/// (başarısız giriş korumasına özel, süre-sınırlı) bilinçli olarak ayrı tutulan, admin'in kalıcı
/// "hesabı devre dışı bırak" kararını temsil eden alan (Task 16 analiz kararı). Varsayılan true —
/// mevcut seed edilen admin/dev-test kullanıcıları ve migration'ın DB DEFAULT'ı ile aktif kalır.</summary>
public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
}
