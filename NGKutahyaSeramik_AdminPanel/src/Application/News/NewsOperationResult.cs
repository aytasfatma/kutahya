namespace Application.News;

public class NewsOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static NewsOperationResult Success() => new() { Succeeded = true };

    public static NewsOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
