namespace Application.Blogs;

public class BlogCategoryOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static BlogCategoryOperationResult Success() => new() { Succeeded = true };

    public static BlogCategoryOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
