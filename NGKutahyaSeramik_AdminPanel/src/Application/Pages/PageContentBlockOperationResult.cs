namespace Application.Pages;

public class PageContentBlockOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static PageContentBlockOperationResult Success() => new() { Succeeded = true };

    public static PageContentBlockOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
