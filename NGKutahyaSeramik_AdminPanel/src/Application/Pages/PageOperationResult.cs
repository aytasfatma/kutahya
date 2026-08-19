namespace Application.Pages;

public class PageOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static PageOperationResult Success() => new() { Succeeded = true };

    public static PageOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
