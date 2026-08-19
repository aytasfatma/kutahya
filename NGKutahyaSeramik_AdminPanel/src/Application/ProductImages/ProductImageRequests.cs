using Domain.Enums;

namespace Application.ProductImages;

/// <summary>
/// Presentation katmanından (IFormFile) türetilmiş, framework'ten bağımsız yükleme isteği.
/// Application/Infrastructure hiçbir ASP.NET Core tipine (IFormFile) bağımlı değildir.
/// </summary>
public class AddProductImageRequest
{
    public int ProductId { get; init; }
    public ProductImageType ImageType { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Length { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}
