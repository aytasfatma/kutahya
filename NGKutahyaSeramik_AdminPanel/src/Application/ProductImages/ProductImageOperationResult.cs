namespace Application.ProductImages;

public class ProductImageOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static ProductImageOperationResult Success() => new() { Succeeded = true };

    public static ProductImageOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
