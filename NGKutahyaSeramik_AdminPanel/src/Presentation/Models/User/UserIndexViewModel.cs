using Application.Users;

namespace Presentation.Models.User;

public class UserIndexViewModel
{
    public IReadOnlyList<UserDto> Users { get; init; } = [];
}
