namespace Presentation.Models.Shared;

public sealed class PaginationViewModel
{
    public string ActionName { get; init; } = "Index";
    public string? ControllerName { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public int TotalItems { get; init; }
    public IReadOnlyDictionary<string, string?> RouteValues { get; init; } = new Dictionary<string, string?>();
    public bool EnableHtmx { get; init; }
    public string? HxTarget { get; init; }
    public string HxSwap { get; init; } = "outerHTML";

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int FirstItem => TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int LastItem => Math.Min(CurrentPage * PageSize, TotalItems);
}
