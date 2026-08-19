using System.Text.RegularExpressions;
using Application.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Identity;

/// <summary>
/// Madde 30 — Kullanıcı Yönetimi (Admin=Tam, diğer 3 rol=—). UserManager/RoleManager doğrudan
/// kullanılıyor (ADR-005: "sıfırdan RBAC yazılmayacak, Identity'nin altyapısı iş gereksinimi olarak
/// sarmalanmayacak") — hiçbir noktada AppDbContext.Users/AspNetUsers'a doğrudan yazma yapılmıyor.
/// Task 16 analiz kararları: parola admin tarafından doğrudan belirlenir (e-posta altyapısı yok),
/// hard-delete izinli, kendi-hesap ve son-aktif-Admin guardrail'leri servis seviyesinde (yalnızca
/// UI'da değil) uygulanıyor. Task 16B implementasyon-öncesi revizyon (2 karar değişikliği):
/// (1) her kullanıcı TAM OLARAK bir role sahiptir — AspNetUserRoles alt yapısı değişmedi, kural
/// yalnızca uygulama seviyesinde (CreateAsync/UpdateAsync) uygulanıyor; (2) Email/UserName
/// oluşturulduktan sonra değiştirilemez — login bilgisi sabit tutulur, UpdateAsync artık
/// SetEmailAsync/SetUserNameAsync çağırmıyor.
/// </summary>
public partial class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly string? _seedAdminEmail;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _seedAdminEmail = configuration["SeedAdmin:Email"];
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            result.Add(await MapToDtoAsync(user));
        }

        return result;
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user is null ? null : await MapToDtoAsync(user);
    }

    public Task<IReadOnlyList<string>> GetAvailableRolesAsync() =>
        Task.FromResult(ApplicationRoles.All);

    public async Task<UserOperationResult> CreateAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex().IsMatch(email))
        {
            return UserOperationResult.Failure("Geçerli bir e-posta adresi zorunludur.");
        }

        var roleValidation = ValidateRole(request.Role);
        if (roleValidation is not null)
        {
            return UserOperationResult.Failure(roleValidation);
        }

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return UserOperationResult.Failure("Bu e-posta adresi zaten kullanılıyor.");
        }

        // Madde 15 (Task 16 analiz): admin tarafından oluşturulan kullanıcı için EmailConfirmed=true —
        // self-servis kayıt olmadığı için "doğrulanmamış e-posta" durumu yanıltıcı olurdu.
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            IsActive = request.IsActive
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return UserOperationResult.Failure(DescribeErrors(createResult.Errors));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            // Telafi: rol ataması başarısız olursa rolsüz-yetim bir kullanıcı bırakmamak için
            // az önce oluşturulan kullanıcı geri alınır (ProductImage/Banner'ın DB-hatasında-dosya-
            // geri-alma telafi desenine karşılık gelen, kullanıcı-oluşturma bağlamındaki eşdeğeri).
            await _userManager.DeleteAsync(user);
            return UserOperationResult.Failure(DescribeErrors(roleResult.Errors));
        }

        return UserOperationResult.Success();
    }

    public async Task<UserOperationResult> UpdateAsync(string id, UpdateUserRequest request, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserOperationResult.Failure("Kullanıcı bulunamadı.");
        }

        var roleValidation = ValidateRole(request.Role);
        if (roleValidation is not null)
        {
            return UserOperationResult.Failure(roleValidation);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();
        var willLoseAdminRole = currentRole == ApplicationRoles.Admin && request.Role != ApplicationRoles.Admin;
        var willDeactivate = user.IsActive && !request.IsActive;

        if (willDeactivate && IsSeedAdmin(user))
        {
            return UserOperationResult.Failure("Ana yönetici hesabı pasifleştirilemez.");
        }

        if (willLoseAdminRole && user.Id == currentUserId)
        {
            return UserOperationResult.Failure("Kendi Admin rolünüzü kaldıramazsınız.");
        }

        if (willDeactivate && user.Id == currentUserId)
        {
            return UserOperationResult.Failure("Kendi hesabınızı pasifleştiremezsiniz.");
        }

        if ((willLoseAdminRole || willDeactivate) && await IsLastActiveAdminAsync(user))
        {
            return UserOperationResult.Failure(willLoseAdminRole
                ? "Sistemdeki son aktif Admin kullanıcının Admin rolü kaldırılamaz."
                : "Sistemdeki son aktif Admin pasifleştirilemez.");
        }

        // Email/UserName kasıtlı olarak burada değiştirilmiyor — oluşturulduktan sonra sabit
        // (login bilgisi, karar değişikliği: Task 16B analiz sonrası).
        user.IsActive = request.IsActive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return UserOperationResult.Failure(DescribeErrors(updateResult.Errors));
        }

        if (currentRole != request.Role)
        {
            if (currentRole is not null)
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRole);
                if (!removeResult.Succeeded)
                {
                    return UserOperationResult.Failure(DescribeErrors(removeResult.Errors));
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addResult.Succeeded)
            {
                return UserOperationResult.Failure(DescribeErrors(addResult.Errors));
            }
        }

        return UserOperationResult.Success();
    }

    public async Task<UserOperationResult> ResetPasswordAsync(string id, ResetUserPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserOperationResult.Failure("Kullanıcı bulunamadı.");
        }

        // Madde 14 (Task 16 analiz): e-posta altyapısı olmadığı için token e-postayla gönderilmiyor —
        // sunucu içinde üretilip aynı istek içinde tüketiliyor. Parola hiçbir yerde loglanmıyor/saklanmıyor.
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        return result.Succeeded
            ? UserOperationResult.Success()
            : UserOperationResult.Failure(DescribeErrors(result.Errors));
    }

    public async Task<UserOperationResult> ActivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserOperationResult.Failure("Kullanıcı bulunamadı.");
        }

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded
            ? UserOperationResult.Success()
            : UserOperationResult.Failure(DescribeErrors(result.Errors));
    }

    public async Task<UserOperationResult> DeactivateAsync(string id, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserOperationResult.Failure("Kullanıcı bulunamadı.");
        }

        if (user.Id == currentUserId)
        {
            return UserOperationResult.Failure("Kendi hesabınızı pasifleştiremezsiniz.");
        }

        if (IsSeedAdmin(user))
        {
            return UserOperationResult.Failure("Ana yönetici hesabı pasifleştirilemez.");
        }

        if (await IsLastActiveAdminAsync(user))
        {
            return UserOperationResult.Failure("Sistemdeki son aktif Admin pasifleştirilemez.");
        }

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded
            ? UserOperationResult.Success()
            : UserOperationResult.Failure(DescribeErrors(result.Errors));
    }

    public async Task<UserOperationResult> DeleteAsync(string id, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserOperationResult.Failure("Kullanıcı bulunamadı.");
        }

        if (user.Id == currentUserId)
        {
            return UserOperationResult.Failure("Kendi hesabınızı silemezsiniz.");
        }

        if (IsSeedAdmin(user))
        {
            return UserOperationResult.Failure("Ana yönetici hesabı silinemez.");
        }

        if (await IsLastActiveAdminAsync(user))
        {
            return UserOperationResult.Failure("Sistemdeki son aktif Admin silinemez.");
        }

        var result = await _userManager.DeleteAsync(user);

        return result.Succeeded
            ? UserOperationResult.Success()
            : UserOperationResult.Failure(DescribeErrors(result.Errors));
    }

    /// <summary>Hedef kullanıcı şu an aktif VE Admin rolündeyse VE ondan başka aktif bir Admin yoksa
    /// true döner — "son aktif Admin" korumasının tek doğruluk kaynağı (Delete/Deactivate/rol-kaldırma
    /// hepsi bunu kullanır).</summary>
    private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user)
    {
        if (!user.IsActive)
        {
            return false;
        }

        if (!await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
        {
            return false;
        }

        var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        return !admins.Any(a => a.Id != user.Id && a.IsActive);
    }

    private static string? ValidateRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "Rol seçilmelidir.";
        }

        if (!ApplicationRoles.All.Contains(role))
        {
            return "Geçersiz rol seçildi.";
        }

        return null;
    }

    private static string DescribeErrors(IEnumerable<IdentityError> errors) =>
        string.Join(" ", errors.Select(e => e.Description));

    private bool IsSeedAdmin(ApplicationUser user) =>
        !string.IsNullOrWhiteSpace(_seedAdminEmail) &&
        string.Equals(user.Email, _seedAdminEmail, StringComparison.OrdinalIgnoreCase);

    private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            IsSeedAdmin = IsSeedAdmin(user),
            Role = roles.FirstOrDefault() ?? string.Empty
        };
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
