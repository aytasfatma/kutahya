namespace Application.News;

public class NewsCategoryOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static NewsCategoryOperationResult Success() => new() { Succeeded = true };

    public static NewsCategoryOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
