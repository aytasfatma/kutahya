namespace Application.ReferenceProjects;

public class ReferenceProjectOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static ReferenceProjectOperationResult Success() => new() { Succeeded = true };

    public static ReferenceProjectOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
