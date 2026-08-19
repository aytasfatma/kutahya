namespace Application.Roles;

/// <summary>Bu sistemde teknik rol adı ve görünen ad ayrı değil — `ApplicationRoles` sabitleri
/// (ör. "İçerik Editörü") hem `IdentityRole.Name` hem `[Authorize(Roles=...)]` string'i olarak
/// doğrudan kullanılıyor (Task 16B'den beri). Bu yüzden `Name` tek alan; icat edilmiş ayrı bir
/// "DisplayName" alanı yok (Task 17 analiz kararı).</summary>
public class RoleDto
{
    public string Name { get; init; } = string.Empty;
    public int ActiveUserCount { get; init; }
    public int TotalUserCount { get; init; }
}
