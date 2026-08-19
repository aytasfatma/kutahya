namespace Application.Users;

public class UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool IsLockedOut { get; init; }
    public bool IsSeedAdmin { get; init; }
    public string Role { get; init; } = string.Empty;
}
