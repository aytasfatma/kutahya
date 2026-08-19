namespace Application.Languages;

public class UpdateLanguageRequest
{
    public string Name { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}
