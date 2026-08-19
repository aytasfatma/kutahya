using System.Linq;
using Application.Roles;

namespace Presentation.Models.Role;

public class RoleDetailViewModel
{
    public RoleDetailDto Role { get; init; } = new();

    public int AssignedUserCount => Role.Users.Count;

    public int ActiveUserCount => Role.Users.Count(user => user.IsActive);

    public int AccessibleModuleCount => Role.Permissions.Count(permission => permission.AccessLevel != RoleAccessLevel.None);
}
