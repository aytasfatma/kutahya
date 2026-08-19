namespace Application.Categories;

public class CategoryOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static CategoryOperationResult Success() => new() { Succeeded = true };

    public static CategoryOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
