namespace Application.Banners;

public class BannerOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static BannerOperationResult Success() => new() { Succeeded = true };

    public static BannerOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
