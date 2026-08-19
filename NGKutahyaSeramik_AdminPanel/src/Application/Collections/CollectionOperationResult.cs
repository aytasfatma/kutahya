namespace Application.Collections;

public class CollectionOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static CollectionOperationResult Success() => new() { Succeeded = true };

    public static CollectionOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
