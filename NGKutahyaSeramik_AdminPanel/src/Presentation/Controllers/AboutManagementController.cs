using System.Text.Json;
using Application.Pages;
using Application.Storage;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.AboutManagement;

namespace Presentation.Controllers;

[Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor)]
public sealed class AboutManagementController : Controller
{
    private const string SeoUrl = "hakkimizda-yonetimi";
    private const long MaxIconUploadBytes = 2 * 1024 * 1024;
    private static readonly string[] AllowedIconExtensions = [".jpg", ".jpeg", ".png", ".webp", ".svg"];
    private static readonly IReadOnlyDictionary<string, string[]> SectionFields = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["header"] = [nameof(AboutManagementViewModel.HeaderEyebrow), nameof(AboutManagementViewModel.HeaderTitle), nameof(AboutManagementViewModel.HeaderDescription)],
        ["vision-mission"] = [nameof(AboutManagementViewModel.VisionTitle), nameof(AboutManagementViewModel.VisionSubtitle), nameof(AboutManagementViewModel.VisionText), nameof(AboutManagementViewModel.MissionTitle), nameof(AboutManagementViewModel.MissionSubtitle), nameof(AboutManagementViewModel.MissionText)],
        ["statistics"] = [nameof(AboutManagementViewModel.StatisticItems)],
        ["history"] = [nameof(AboutManagementViewModel.HistoryTitle), nameof(AboutManagementViewModel.HistoryDescription), nameof(AboutManagementViewModel.HistoryItems)],
        ["values"] = [nameof(AboutManagementViewModel.ValuesTitle), nameof(AboutManagementViewModel.ValuesDescription), nameof(AboutManagementViewModel.Values)],
        ["production"] = [nameof(AboutManagementViewModel.ProductionTitle), nameof(AboutManagementViewModel.ProductionText), nameof(AboutManagementViewModel.ProductionItems)],
        ["awards"] = [nameof(AboutManagementViewModel.AwardsTitle), nameof(AboutManagementViewModel.AwardsDescription), nameof(AboutManagementViewModel.Awards)],
        ["certificates"] = [nameof(AboutManagementViewModel.CertificatesTitle), nameof(AboutManagementViewModel.CertificatesDescription)],
        ["partnerships"] = [nameof(AboutManagementViewModel.PartnershipsTitle), nameof(AboutManagementViewModel.PartnershipsDescription), nameof(AboutManagementViewModel.Partnerships)],
        ["information"] = [nameof(AboutManagementViewModel.InformationTitle), nameof(AboutManagementViewModel.InformationDescription)]
    };
    private readonly PageService _pages;
    private readonly PageContentBlockService _blocks;
    private readonly IFileStorageService _fileStorage;
    public AboutManagementController(PageService pages, PageContentBlockService blocks, IFileStorageService fileStorage)
    {
        _pages = pages;
        _blocks = blocks;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string section = "header")
    {
        section = NormalizeSection(section);
        var (page, block) = await EnsureAsync();
        SetSectionViewData(section);
        if (page is null || block is null) return View(new AboutManagementViewModel());
        var content = block.Translations.FirstOrDefault(x => x.LanguageCode == "TR")?.Content;
        return View(Deserialize(content));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxIconUploadBytes * 6 + 1024 * 1024)]
    public async Task<IActionResult> Index(string section, AboutManagementViewModel model)
    {
        section = NormalizeSection(section);
        SetSectionViewData(section);
        if (!ModelState.IsValid) return View(model);
        var (page, block) = await EnsureAsync();
        if (page is null || block is null) return View(model);
        var content = block.Translations.FirstOrDefault(x => x.LanguageCode == "TR")?.Content;
        var completeModel = Deserialize(content);
        if (section == "statistics")
        {
            var iconError = await ApplyStatisticIconsAsync(model.StatisticItems, completeModel.StatisticItems, Request.Form.Files);
            if (iconError is not null)
            {
                this.AddOperationError(iconError);
                model.StatisticItems = completeModel.StatisticItems;
                return View(model);
            }
        }
        CopySection(model, completeModel, section);
        var languages = await _pages.GetActiveLanguagesAsync();
        var result = await _blocks.UpdateAsync(page.Id, block.Id, new UpdatePageContentBlockRequest
        {
            BlockType = PageBlockType.Tab,
            IsActive = true,
            Translations = languages.Select(language => new PageContentBlockTranslationInput
            {
                LanguageId = language.Id,
                Title = language.Code == "TR" ? "Hakkımızda İçeriği" : null,
                Content = language.Code == "TR" ? JsonSerializer.Serialize(completeModel) : null
            }).ToList()
        });
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Hakkımızda içeriği güncellendi." : result.ErrorMessage;
        return result.Succeeded ? RedirectToAction(nameof(Index), new { section }) : View(model);
    }

    private async Task<string?> ApplyStatisticIconsAsync(
        List<AboutStatisticItem> postedItems,
        List<AboutStatisticItem> existingItems,
        IFormFileCollection uploadedFiles)
    {
        for (var i = 0; i < postedItems.Count; i++)
        {
            var item = postedItems[i];
            var existingIconPath = i < existingItems.Count ? existingItems[i].IconPath : null;
            var file = uploadedFiles.GetFile($"statisticIcons[{i}]");

            if (item.RemoveIcon)
            {
                if (!string.IsNullOrWhiteSpace(existingIconPath)) _fileStorage.Delete(existingIconPath);
                item.IconPath = null;
                continue;
            }

            if (file is { Length: > 0 })
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedIconExtensions.Contains(extension))
                {
                    return "Logo yalnızca JPG, JPEG, PNG, WEBP veya SVG formatında olabilir.";
                }

                if (file.Length > MaxIconUploadBytes)
                {
                    return "Logo dosyası en fazla 2 MB olabilir.";
                }

                await using var stream = file.OpenReadStream();
                var savedPath = await _fileStorage.SaveAsync("about/statistics", stream, $"{Guid.NewGuid():N}{extension}");
                if (!string.IsNullOrWhiteSpace(existingIconPath)) _fileStorage.Delete(existingIconPath);
                item.IconPath = savedPath;
                continue;
            }

            item.IconPath = existingIconPath;
        }

        return null;
    }

    private static string NormalizeSection(string? section) =>
        section is not null && SectionFields.ContainsKey(section) ? section : "header";

    private void SetSectionViewData(string section) => ViewData["SectionKey"] = section;

    private static void CopySection(AboutManagementViewModel source, AboutManagementViewModel target, string section)
    {
        foreach (var propertyName in SectionFields[section])
        {
            var property = typeof(AboutManagementViewModel).GetProperty(propertyName);
            property?.SetValue(target, property.GetValue(source));
        }
    }

    private async Task<(PageDto? Page, PageContentBlockDto? Block)> EnsureAsync()
    {
        var page = (await _pages.GetAllAsync()).FirstOrDefault(p => p.Translations.Any(t => string.Equals(t.SeoUrl, SeoUrl, StringComparison.OrdinalIgnoreCase)));
        if (page is null)
        {
            var languages = await _pages.GetActiveLanguagesAsync();
            var created = await _pages.CreateAsync(new CreatePageRequest { Translations = languages.Select(language => new PageTranslationInput { LanguageId = language.Id, Title = language.Code == "TR" ? "Hakkımızda Yönetimi" : null, SeoUrl = language.Code == "TR" ? SeoUrl : null }).ToList() });
            if (!created.Succeeded) { this.AddOperationError(created.ErrorMessage); return (null, null); }
            page = (await _pages.GetAllAsync()).FirstOrDefault(p => p.Translations.Any(t => string.Equals(t.SeoUrl, SeoUrl, StringComparison.OrdinalIgnoreCase)));
        }
        if (page is null) return (null, null);
        var block = (await _blocks.GetByPageIdAsync(page.Id)).FirstOrDefault(x => x.BlockType == PageBlockType.Tab);
        if (block is null)
        {
            var languages = await _pages.GetActiveLanguagesAsync();
            var added = await _blocks.AddAsync(new AddPageContentBlockRequest { PageId = page.Id, BlockType = PageBlockType.Tab, IsActive = true, Translations = languages.Select(language => new PageContentBlockTranslationInput { LanguageId = language.Id, Title = language.Code == "TR" ? "Hakkımızda İçeriği" : null, Content = language.Code == "TR" ? JsonSerializer.Serialize(new AboutManagementViewModel()) : null }).ToList() });
            if (!added.Succeeded) { this.AddOperationError(added.ErrorMessage); return (page, null); }
            block = (await _blocks.GetByPageIdAsync(page.Id)).FirstOrDefault(x => x.BlockType == PageBlockType.Tab);
        }
        return (page, block);
    }

    private static AboutManagementViewModel Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<AboutManagementViewModel>(json) ?? new(); }
        catch { return new(); }
    }
}
