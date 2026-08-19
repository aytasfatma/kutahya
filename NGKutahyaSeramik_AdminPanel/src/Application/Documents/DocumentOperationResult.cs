namespace Application.Documents;

public class DocumentOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static DocumentOperationResult Success() => new() { Succeeded = true };

    public static DocumentOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
