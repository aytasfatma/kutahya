namespace Application.Dealers;

public class DealerOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static DealerOperationResult Success() => new() { Succeeded = true };

    public static DealerOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
