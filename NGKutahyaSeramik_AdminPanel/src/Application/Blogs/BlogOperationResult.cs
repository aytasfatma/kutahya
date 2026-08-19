namespace Application.Blogs;

public class BlogOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static BlogOperationResult Success() => new() { Succeeded = true };

    public static BlogOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
