namespace Application.ReferenceProjects;

/// <summary>
/// Presentation katmanından (IFormFile) türetilmiş, framework'ten bağımsız yükleme isteği
/// (ProductImages/AddProductImageRequest ile aynı desen).
/// </summary>
public class AddReferenceProjectImageRequest
{
    public int ReferenceProjectId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Length { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}
