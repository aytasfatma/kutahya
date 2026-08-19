namespace Application.Users;

public class UserOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static UserOperationResult Success() => new() { Succeeded = true };

    public static UserOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
