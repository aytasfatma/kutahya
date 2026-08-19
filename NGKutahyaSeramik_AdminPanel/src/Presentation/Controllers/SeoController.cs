using Application.Seo;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Seo;

namespace Presentation.Controllers;

/// <summary>
/// Backlog #4 — SEO Yönetimi. Madde 7.2/30'un SEO Editörü tanımı ("Meta alanları, URL yönetimi...")
/// bu modülün doğrudan karşılığı — Admin+SEO Editörü tam yetkili, diğer 2 rolün (İçerik Editörü/
/// Ürün Yöneticisi) bu MERKEZİ ekrana erişimi yok (kendi modüllerindeki SEO alanlarına Task 20'nin
/// alan-seviyeli RBAC'ı üzerinden zaten erişebiliyorlar — burada tekrar erişim vermek gereksiz).
/// Public sitemap/robots.txt/redirect YOK (ADR-001/002/009, görev talimatı) — yalnızca admin CRUD.
/// </summary>
[Authorize(Roles = SeoController.EditRoles)]
public class SeoController : Controller
{
    internal const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.SeoEditor;

    private readonly SeoManagementService _seoService;

    public SeoController(SeoManagementService seoService)
    {
        _seoService = seoService;
    }

    public async Task<IActionResult> Index(EntityType? type)
    {
        if (type is not null && !_seoService.IsSupported(type.Value))
        {
            TempData["ErrorMessage"] = "Bu içerik türü SEO Yönetimi tarafından desteklenmiyor.";
            return RedirectToAction(nameof(Index));
        }

        var model = await BuildIndexViewModelAsync(type);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id, EntityType type)
    {
        // Index'in zaten yaptığı IsSupported kontrolü burada da gerekliydi — Index route dışından
        // (ör. doğrudan URL ile ?type=Banner/Dealer) çağrıldığında GetRecordDetailAsync/GetRecordsAsync'in
        // desteklenmeyen tipler için attığı InvalidOperationException'ı burada YAKALAMAK yerine (o iç
        // invariant'ı zayıflatmadan) çağrı yapılmadan ÖNCE aynı dostane redirect'i uyguluyoruz.
        if (!_seoService.IsSupported(type))
        {
            TempData["ErrorMessage"] = "Bu içerik türü SEO Yönetimi tarafından desteklenmiyor.";
            return RedirectToAction(nameof(Index));
        }

        var detail = await _seoService.GetRecordDetailAsync(type, id);
        if (detail is null)
        {
            TempData["ErrorMessage"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Index), new { type });
        }

        var model = new SeoEditViewModel
        {
            EntityType = detail.EntityType,
            EntityId = detail.EntityId,
            DisplayName = detail.DisplayName,
            SupportsMetaFields = detail.SupportsMetaFields,
            Languages = detail.Languages.Select(l => new SeoLanguageFieldViewModel
            {
                LanguageId = l.LanguageId,
                LanguageCode = l.LanguageCode,
                LanguageName = l.LanguageName,
                SeoUrl = l.SeoUrl,
                MetaTitle = l.MetaTitle,
                MetaDescription = l.MetaDescription
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EntityType type, SeoEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.EntityType = type;
            model.EntityId = id;
            return View(model);
        }

        var anyDuplicateWarning = false;

        foreach (var language in model.Languages)
        {
            var (succeeded, errorMessage, duplicateWarning) = await _seoService.UpdateAsync(
                type, id, language.LanguageId, language.SeoUrl, language.MetaTitle, language.MetaDescription);

            if (!succeeded)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                model.EntityType = type;
                model.EntityId = id;
                return View(model);
            }

            anyDuplicateWarning = anyDuplicateWarning || duplicateWarning;
        }

        TempData["SuccessMessage"] = "SEO bilgileri güncellendi.";
        if (anyDuplicateWarning)
        {
            TempData["WarningMessage"] = "Uyarı: Bu SEO URL, aynı içerik türü ve dilde başka bir kayıtla çakışıyor. " +
                "Kaydetme işlemi engellenmedi ama düzeltmenizi öneririz (bkz. Duplicate SEO URL raporu).";
        }

        return RedirectToAction(nameof(Edit), new { id, type });
    }

    public async Task<IActionResult> MissingReport()
    {
        var report = await _seoService.GetMissingSeoReportAsync();
        return View(report);
    }

    public async Task<IActionResult> Duplicates()
    {
        var report = await _seoService.GetDuplicateSeoUrlReportAsync();
        return View(report);
    }

    private async Task<SeoIndexViewModel> BuildIndexViewModelAsync(EntityType? selectedType)
    {
        var contentTypes = _seoService.GetSupportedContentTypes()
            .Select(type => new SeoIndexContentTypeViewModel
            {
                EntityType = type.EntityType,
                Label = type.Label,
                SupportsMetaFields = type.SupportsMetaFields,
                IsSelected = selectedType == type.EntityType
            })
            .ToList();

        var recordsByType = new Dictionary<EntityType, IReadOnlyList<SeoRecordSummaryDto>>();
        foreach (var type in contentTypes)
        {
            recordsByType[type.EntityType] = await _seoService.GetRecordsAsync(type.EntityType);
        }

        var missingRows = await _seoService.GetMissingSeoReportAsync();
        var duplicateGroups = await _seoService.GetDuplicateSeoUrlReportAsync();

        IReadOnlyList<SeoIndexRowViewModel> rows = [];
        string? selectedTypeLabel = null;
        if (selectedType is not null)
        {
            var selectedContentType = contentTypes.First(type => type.EntityType == selectedType.Value);
            selectedTypeLabel = selectedContentType.Label;
            rows = await BuildIndexRowsAsync(selectedContentType, recordsByType[selectedType.Value], duplicateGroups);
        }

        return new SeoIndexViewModel
        {
            ContentTypes = contentTypes,
            SelectedType = selectedType,
            SelectedTypeLabel = selectedTypeLabel,
            Summary = new SeoIndexSummaryViewModel
            {
                TotalRecords = recordsByType.Values.Sum(records => records.Count),
                MissingRows = missingRows.Count,
                DuplicateGroups = duplicateGroups.Count,
                SeoUrlOnlyRecords = contentTypes
                    .Where(type => !type.SupportsMetaFields)
                    .Sum(type => recordsByType[type.EntityType].Count)
            },
            Rows = rows
        };
    }

    private async Task<IReadOnlyList<SeoIndexRowViewModel>> BuildIndexRowsAsync(
        SeoIndexContentTypeViewModel contentType,
        IReadOnlyList<SeoRecordSummaryDto> records,
        IReadOnlyList<SeoDuplicateGroupDto> duplicateGroups)
    {
        var duplicateLookup = new HashSet<string>(
            duplicateGroups
                .Where(group => group.EntityType == contentType.EntityType)
                .SelectMany(group => group.Records.Select(record => BuildDuplicateKey(group.EntityType, record.EntityId, group.LanguageCode, group.NormalizedSeoUrl))));

        var rows = new List<SeoIndexRowViewModel>();
        foreach (var record in records)
        {
            var detail = await _seoService.GetRecordDetailAsync(contentType.EntityType, record.EntityId);
            if (detail is null)
            {
                continue;
            }

            foreach (var language in detail.Languages)
            {
                var normalizedSeoUrl = SeoUrlNormalizer.Normalize(language.SeoUrl);
                var missingSeoUrl = string.IsNullOrWhiteSpace(language.SeoUrl);
                var missingMetaTitle = detail.SupportsMetaFields && string.IsNullOrWhiteSpace(language.MetaTitle);
                var missingMetaDescription = detail.SupportsMetaFields && string.IsNullOrWhiteSpace(language.MetaDescription);
                var isDuplicateUrl = !missingSeoUrl &&
                    duplicateLookup.Contains(BuildDuplicateKey(contentType.EntityType, record.EntityId, language.LanguageCode, normalizedSeoUrl));

                var (healthLabel, healthBadgeClass) = ResolveHealth(missingSeoUrl, missingMetaTitle, missingMetaDescription, isDuplicateUrl);

                rows.Add(new SeoIndexRowViewModel
                {
                    EntityType = contentType.EntityType,
                    EntityId = record.EntityId,
                    DisplayName = record.DisplayName,
                    ContentTypeLabel = contentType.Label,
                    LanguageId = language.LanguageId,
                    LanguageCode = language.LanguageCode,
                    LanguageName = language.LanguageName,
                    SeoUrl = language.SeoUrl,
                    SupportsMetaFields = detail.SupportsMetaFields,
                    MissingSeoUrl = missingSeoUrl,
                    MissingMetaTitle = missingMetaTitle,
                    MissingMetaDescription = missingMetaDescription,
                    IsDuplicateUrl = isDuplicateUrl,
                    HealthLabel = healthLabel,
                    HealthBadgeClass = healthBadgeClass
                });
            }
        }

        return rows
            .OrderBy(row => row.DisplayName)
            .ThenBy(row => row.LanguageCode)
            .ToList();
    }

    private static (string Label, string BadgeClass) ResolveHealth(
        bool missingSeoUrl,
        bool missingMetaTitle,
        bool missingMetaDescription,
        bool isDuplicateUrl)
    {
        if (missingSeoUrl)
        {
            return ("Eksik", "bg-danger-lt text-danger");
        }

        if (isDuplicateUrl)
        {
            return ("Duplicate", "bg-warning-lt text-warning");
        }

        if (missingMetaTitle || missingMetaDescription)
        {
            return ("Uyarı", "bg-warning-lt text-warning");
        }

        return ("Tam", "bg-success-lt text-success");
    }

    private static string BuildDuplicateKey(EntityType entityType, int entityId, string languageCode, string normalizedSeoUrl) =>
        $"{entityType}|{entityId}|{languageCode}|{normalizedSeoUrl}";
}
