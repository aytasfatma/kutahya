namespace Application.Users;

/// <summary>
/// Madde 30 — Kullanıcı Yönetimi (Admin-only). Bu servisin ARAYÜZÜ Application katmanında, ama
/// implementasyonu (ApplicationUser/UserManager/RoleManager'a bağımlı olduğu için) Infrastructure
/// katmanında yaşıyor — projedeki her Repository'nin arayüz-Application/implementasyon-Infrastructure
/// deseniyle birebir tutarlı (Application projesi Infrastructure'a referans veremez, ApplicationUser
/// Infrastructure.Identity'de tanımlı). Bu, projedeki interface kullanan ilk ve tek servis — sebep
/// mimari katman sınırı, stil tercihi değil.
/// </summary>
public interface IUserManagementService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync();

    Task<UserDto?> GetByIdAsync(string id);

    Task<IReadOnlyList<string>> GetAvailableRolesAsync();

    Task<UserOperationResult> CreateAsync(CreateUserRequest request);

    Task<UserOperationResult> UpdateAsync(string id, UpdateUserRequest request, string currentUserId);

    Task<UserOperationResult> ResetPasswordAsync(string id, ResetUserPasswordRequest request);

    Task<UserOperationResult> ActivateAsync(string id);

    Task<UserOperationResult> DeactivateAsync(string id, string currentUserId);

    Task<UserOperationResult> DeleteAsync(string id, string currentUserId);
}
