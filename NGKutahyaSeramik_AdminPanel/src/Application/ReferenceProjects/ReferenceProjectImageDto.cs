namespace Application.ReferenceProjects;

public class ReferenceProjectImageDto
{
    public int Id { get; init; }
    public int ReferenceProjectId { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public bool IsFeatured { get; init; }
    public int DisplayOrder { get; init; }
}
