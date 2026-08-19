namespace Presentation.Models.Api;

public sealed record PublicDealerImageResponse(int Id, string Url, bool IsFeatured, int DisplayOrder);

public sealed record PublicDealerResponse(
    int Id, string Name, string City, string? District, string? Address,
    string? Phone, string? Email, string? WorkingHours, string Category, string CategoryLabel,
    decimal? Latitude, decimal? Longitude, string? Region, string? RegionName,
    string? FeaturedImageUrl, IReadOnlyList<PublicDealerImageResponse> Images,
    IReadOnlyList<string> Brands);

public sealed record PublicLanguageResponse(int Id, string Code, string Name, int DisplayOrder);

public sealed class PublicFormSubmissionRequest
{
    public string FormType { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Company { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool ConsentAccepted { get; init; }
    public string? Subject { get; init; }
    public string? ProductCode { get; init; }
    public string? ProductName { get; init; }
    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public int? Quantity { get; init; }
}

public sealed record PublicFormSubmissionResponse(bool Succeeded, string Message);

public sealed class PublicCareerSubmissionRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool ConsentAccepted { get; init; }
    public IFormFile? Cv { get; init; }
}
