using Domain.Enums;

namespace Application.Dealers;

public class DealerQuery
{
    public DealerCategory? Category { get; init; }
    public string? City { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}
