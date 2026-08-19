using Application.Translations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Language;

public class LanguageReportQueryViewModel
{
    public EntityType? Type { get; init; }
    public int? EntityId { get; init; }
    public int? LanguageId { get; init; }
    public string? Field { get; init; }
    public string? Search { get; init; }
    public string SortBy { get; init; } = "module";
    public string SortDirection { get; init; } = "asc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class LanguageReportViewModel
{
    public TranslationCoverageReportDto Report { get; init; } = new();
    public LanguageReportQueryViewModel Query { get; init; } = new();
    public IReadOnlyList<MissingTranslationDto> Items { get; init; } = [];
    public IReadOnlyList<MissingTranslationDto> PagedItems { get; init; } = [];
    public IReadOnlyList<LanguageReportRowViewModel> PagedRows { get; init; } = [];
    public IReadOnlyList<SelectListItem> ModuleOptions { get; init; } = [];
    public IReadOnlyList<SelectListItem> RecordOptions { get; init; } = [];
    public IReadOnlyList<SelectListItem> LanguageOptions { get; init; } = [];
    public IReadOnlyList<SelectListItem> FieldOptions { get; init; } = [];
    public int TotalFiltered { get; init; }
    public int TotalPages => Query.PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalFiltered / (double)Query.PageSize);
    public int CurrentPage => Query.Page;
    public string? MostMissingLanguage =>
        Report.ByLanguage.OrderByDescending(x => x.MissingCount).FirstOrDefault(x => x.MissingCount > 0)?.LanguageName;
    public string? MostMissingModule =>
        Report.ByModule.OrderByDescending(x => x.MissingCount).FirstOrDefault(x => x.MissingCount > 0)?.ModuleLabel;
}

public class LanguageReportRowViewModel
{
    public MissingTranslationDto Item { get; init; } = new();
    public string EditUrl { get; init; } = "#";
}
