using Application.Languages;
using Application.Translations;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Language;

namespace Presentation.Controllers;

[Authorize(Roles = ApplicationRoles.Admin)]
public class LanguageController : Controller
{
    private readonly LanguageService _languageService;
    private readonly TranslationCoverageService _translationCoverageService;

    public LanguageController(LanguageService languageService, TranslationCoverageService translationCoverageService)
    {
        _languageService = languageService;
        _translationCoverageService = translationCoverageService;
    }

    public async Task<IActionResult> Index()
    {
        var languages = await _languageService.GetAllAsync();
        return View(languages);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var language = await _languageService.GetByIdAsync(id);
        if (language is null)
        {
            TempData["ErrorMessage"] = "Dil bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new LanguageEditViewModel
        {
            Id = language.Id,
            Code = language.Code,
            Name = language.Name,
            DisplayOrder = language.DisplayOrder,
            IsActive = language.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LanguageEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        LanguageOperationResult result;
        try
        {
            result = await _languageService.UpdateAsync(id, new UpdateLanguageRequest
            {
                Name = model.Name,
                DisplayOrder = model.DisplayOrder!.Value,
                IsActive = model.IsActive
            });
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = LanguageOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            var current = await _languageService.GetByIdAsync(id);
            model.Code = current?.Code ?? model.Code;
            return View(model);
        }

        TempData["SuccessMessage"] = "Dil güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Report(
        EntityType? type,
        int? entityId,
        int? languageId,
        string? field,
        string? search,
        string sortBy = "module",
        string sortDirection = "asc",
        int page = 1)
    {
        var report = await _translationCoverageService.GetReportAsync();
        var query = NormalizeQuery(type, entityId, languageId, field, search, sortBy, sortDirection, page);
        var filteredItems = ApplyQuery(report.Items, query).ToList();
        var sortedItems = ApplySort(filteredItems, query).ToList();
        var pagedItems = sortedItems
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var model = new LanguageReportViewModel
        {
            Report = report,
            Query = query,
            Items = sortedItems,
            PagedItems = pagedItems,
            PagedRows = pagedItems.Select(item => new LanguageReportRowViewModel
            {
                Item = item,
                EditUrl = BuildEditUrl(item, absolute: false)
            }).ToList(),
            TotalFiltered = sortedItems.Count,
            ModuleOptions = BuildModuleOptions(query.Type),
            RecordOptions = BuildRecordOptions(report.Items, query.Type, query.EntityId),
            LanguageOptions = BuildLanguageOptions(report.ByLanguage, query.LanguageId),
            FieldOptions = BuildFieldOptions(report.Items, query.Type, query.Field)
        };

        return View(model);
    }

    public async Task<IActionResult> ReportExportExcel(
        EntityType? type,
        int? entityId,
        int? languageId,
        string? field,
        string? search,
        string sortBy = "module",
        string sortDirection = "asc")
    {
        var report = await _translationCoverageService.GetReportAsync();
        var query = NormalizeQuery(type, entityId, languageId, field, search, sortBy, sortDirection, 1);
        var items = ApplySort(ApplyQuery(report.Items, query), query).ToList();
        var bytes = LanguageReportExport.ToExcel(items, item => BuildEditUrl(item, absolute: true));
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"eksik-ceviri-raporu-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }

    public async Task<IActionResult> ReportExportPdf(
        EntityType? type,
        int? entityId,
        int? languageId,
        string? field,
        string? search,
        string sortBy = "module",
        string sortDirection = "asc")
    {
        var report = await _translationCoverageService.GetReportAsync();
        var query = NormalizeQuery(type, entityId, languageId, field, search, sortBy, sortDirection, 1);
        var items = ApplySort(ApplyQuery(report.Items, query), query).ToList();
        var bytes = LanguageReportExport.ToPdf(items, query, item => BuildEditUrl(item, absolute: true));
        return File(bytes, "application/pdf", $"eksik-ceviri-raporu-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
    }

    private static LanguageReportQueryViewModel NormalizeQuery(
        EntityType? type,
        int? entityId,
        int? languageId,
        string? field,
        string? search,
        string sortBy,
        string sortDirection,
        int page)
    {
        var normalizedSortBy = sortBy?.ToLowerInvariant() is "record" or "language" or "field" ? sortBy!.ToLowerInvariant() : "module";
        var normalizedDirection = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

        return new LanguageReportQueryViewModel
        {
            Type = type,
            EntityId = null,
            LanguageId = languageId,
            Field = string.IsNullOrWhiteSpace(field) ? null : field,
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            SortBy = normalizedSortBy,
            SortDirection = normalizedDirection,
            Page = Math.Max(1, page),
            PageSize = 20
        };
    }

    private static IEnumerable<MissingTranslationDto> ApplyQuery(IEnumerable<MissingTranslationDto> items, LanguageReportQueryViewModel query)
    {
        if (query.Type is not null)
        {
            items = items.Where(i => i.EntityType == query.Type.Value);
        }

        if (query.EntityId is not null)
        {
            items = items.Where(i => i.EntityId == query.EntityId.Value);
        }

        if (query.LanguageId is not null)
        {
            items = items.Where(i => i.LanguageId == query.LanguageId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Field))
        {
            items = items.Where(i => string.Equals(i.FieldName, query.Field, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            items = items.Where(i =>
                i.DisplayName.Contains(query.Search, StringComparison.CurrentCultureIgnoreCase) ||
                i.ModuleLabel.Contains(query.Search, StringComparison.CurrentCultureIgnoreCase) ||
                i.LanguageName.Contains(query.Search, StringComparison.CurrentCultureIgnoreCase) ||
                i.LanguageCode.Contains(query.Search, StringComparison.CurrentCultureIgnoreCase) ||
                LanguageReportLabels.Field(i.FieldName).Contains(query.Search, StringComparison.CurrentCultureIgnoreCase));
        }

        return items;
    }

    private static IEnumerable<MissingTranslationDto> ApplySort(IEnumerable<MissingTranslationDto> items, LanguageReportQueryViewModel query)
    {
        Func<MissingTranslationDto, object> key = query.SortBy switch
        {
            "record" => i => i.DisplayName,
            "language" => i => i.LanguageCode,
            "field" => i => LanguageReportLabels.Field(i.FieldName),
            _ => i => i.ModuleLabel
        };

        var sorted = query.SortDirection == "desc" ? items.OrderByDescending(key) : items.OrderBy(key);
        return sorted.ThenBy(i => i.DisplayName).ThenBy(i => i.LanguageCode).ThenBy(i => i.FieldName);
    }

    private IReadOnlyList<SelectListItem> BuildModuleOptions(EntityType? selected)
    {
        var options = new List<SelectListItem> { new("Tümü", string.Empty, selected is null) };
        options.AddRange(_translationCoverageService.GetSupportedTypes()
            .Select(t => new SelectListItem(t.Label, t.Type.ToString(), selected == t.Type)));
        return options;
    }

    private static IReadOnlyList<SelectListItem> BuildRecordOptions(IReadOnlyList<MissingTranslationDto> items, EntityType? selectedType, int? selectedEntityId)
    {
        var options = new List<SelectListItem> { new("Tümü", string.Empty, selectedEntityId is null) };
        if (selectedType is null)
        {
            return options;
        }

        options.AddRange(items
            .Where(i => i.EntityType == selectedType)
            .GroupBy(i => new { i.EntityId, i.DisplayName })
            .OrderBy(g => g.Key.DisplayName)
            .Select(g => new SelectListItem(g.Key.DisplayName, g.Key.EntityId.ToString(), selectedEntityId == g.Key.EntityId)));
        return options;
    }

    private static IReadOnlyList<SelectListItem> BuildLanguageOptions(IReadOnlyList<TranslationCoverageByLanguageDto> languages, int? selectedLanguageId)
    {
        var options = new List<SelectListItem> { new("Tümü", string.Empty, selectedLanguageId is null) };
        options.AddRange(languages.Select(l => new SelectListItem($"{l.LanguageCode} - {l.LanguageName}", l.LanguageId.ToString(), selectedLanguageId == l.LanguageId)));
        return options;
    }

    private static IReadOnlyList<SelectListItem> BuildFieldOptions(IReadOnlyList<MissingTranslationDto> items, EntityType? selectedType, string? selectedField)
    {
        var options = new List<SelectListItem> { new("Tümü", string.Empty, string.IsNullOrWhiteSpace(selectedField)) };
        var fields = items
            .Where(i => selectedType is null || i.EntityType == selectedType)
            .Select(i => i.FieldName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(LanguageReportLabels.Field);

        options.AddRange(fields.Select(f => new SelectListItem(LanguageReportLabels.Field(f), f, string.Equals(selectedField, f, StringComparison.OrdinalIgnoreCase))));
        return options;
    }

    private string BuildEditUrl(MissingTranslationDto item, bool absolute)
    {
        var controller = item.EntityType switch
        {
            EntityType.Product => "Product",
            EntityType.Category => "Category",
            EntityType.Collection => "Collection",
            EntityType.Blog => "Blog",
            EntityType.BlogCategory => "BlogCategory",
            EntityType.News => "News",
            EntityType.NewsCategory => "NewsCategory",
            EntityType.Page => "Page",
            EntityType.PageContentBlock => "PageContentBlock",
            EntityType.Banner => "Banner",
            EntityType.ReferenceProject => "ReferenceProject",
            _ => "Language"
        };

        object values = item.EntityType == EntityType.PageContentBlock
            ? new { pageId = item.ParentEntityId, blockId = item.EntityId, languageId = item.LanguageId }
            : new { id = item.EntityId, languageId = item.LanguageId };

        return absolute
            ? Url.Action("Edit", controller, values, Request.Scheme) ?? "#"
            : Url.Action("Edit", controller, values) ?? "#";
    }
}
