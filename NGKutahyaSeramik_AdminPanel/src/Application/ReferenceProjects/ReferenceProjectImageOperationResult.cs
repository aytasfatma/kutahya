namespace Application.ReferenceProjects;

public class ReferenceProjectImageOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static ReferenceProjectImageOperationResult Success() => new() { Succeeded = true };

    public static ReferenceProjectImageOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
