namespace Application.Users;

public class CreateUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

/// <summary>Email/UserName burada kasıtlı olarak yok — oluşturulduktan sonra değiştirilemezler
/// (login bilgisi sabit tutulur, SecurityStamp/NormalizeUserName senkronizasyonu gereksiz karmaşıklık
/// yaratır). Yalnızca Role ve IsActive güncellenebilir.</summary>
public class UpdateUserRequest
{
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public class ResetUserPasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
