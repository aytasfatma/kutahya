namespace Application.Products;

public class ProductOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }
    public int? EntityId { get; private init; }

    public static ProductOperationResult Success(int? entityId = null) => new() { Succeeded = true, EntityId = entityId };

    public static ProductOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
