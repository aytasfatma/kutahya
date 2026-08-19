using Application.Roles;

namespace Presentation.Models.Role;

public class RoleIndexViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; init; } = [];
}
