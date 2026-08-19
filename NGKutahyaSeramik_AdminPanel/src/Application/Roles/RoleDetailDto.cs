namespace Application.Roles;

public class RoleDetailDto
{
    public string Name { get; init; } = string.Empty;

    /// <summary>Madde 7.2'nin rol-yetki kapsamı cümlesi (doğrudan doküman metni, icat edilmedi).</summary>
    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<RoleUserDto> Users { get; init; } = [];
    public int ActiveUserCount { get; init; }
    public int TotalUserCount { get; init; }
    public IReadOnlyList<RolePermissionDto> Permissions { get; init; } = [];
}
