using Application.Pages;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Recording;

namespace Presentation.Controllers;

[Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor)]
public class RecordingController : Controller
{
    private const string ManagedPageSeoUrl = "ng-kutahya-seramik-video";
    private const string ManagedPageTitle = "NG Kütahya Seramik Video Alanı";

    private readonly PageService _pageService;
    private readonly PageContentBlockService _blockService;

    public RecordingController(PageService pageService, PageContentBlockService blockService)
    {
        _pageService = pageService;
        _blockService = blockService;
    }

    public async Task<IActionResult> Index()
    {
        var page = await FindManagedPageAsync();
        if (page is null) return View(Array.Empty<RecordingListItemViewModel>());

        var items = (await _blockService.GetByPageIdAsync(page.Id))
            .Where(x => x.BlockType == PageBlockType.VideoEmbed)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x =>
            {
                var tr = x.Translations.FirstOrDefault(t => t.LanguageCode == "TR");
                return new RecordingListItemViewModel
                {
                    Id = x.Id,
                    Title = tr?.Title ?? "Başlıksız kayıt",
                    Description = tr?.Content,
                    VideoUrl = x.VideoEmbedUrl,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                };
            }).ToList();

        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View(new RecordingFormViewModel { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecordingFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.VideoUrl))
            ModelState.AddModelError(nameof(model.VideoUrl), "Video bağlantısı zorunludur.");
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "Başlık zorunludur.");
        if (!ModelState.IsValid) return View(model);

        var page = await EnsureManagedPageAsync(model.Eyebrow);
        if (page is null) return View(model);

        var result = await _blockService.AddAsync(new AddPageContentBlockRequest
        {
            PageId = page.Id,
            BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = model.VideoUrl,
            IsActive = false,
            EnforceExclusiveActivation = true,
            Translations = await BuildTranslationsAsync(model)
        });

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Kayıt eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var page = await FindManagedPageAsync();
        if (page is null) return NotFound();
        var block = await _blockService.GetByIdAsync(page.Id, id);
        if (block is null || block.BlockType != PageBlockType.VideoEmbed) return NotFound();
        var tr = block.Translations.FirstOrDefault(x => x.LanguageCode == "TR");
        var trPage = page.Translations.FirstOrDefault(x => x.LanguageCode == "TR");
        return View(new RecordingFormViewModel
        {
            Id = block.Id,
            VideoUrl = block.VideoEmbedUrl,
            Eyebrow = trPage?.MetaTitle,
            Title = tr?.Title,
            Description = tr?.Content,
            IsActive = block.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RecordingFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.VideoUrl))
            ModelState.AddModelError(nameof(model.VideoUrl), "Video bağlantısı zorunludur.");
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "Başlık zorunludur.");
        if (!ModelState.IsValid) return View(model);

        var page = await FindManagedPageAsync();
        if (page is null) return NotFound();
        var currentBlock = await _blockService.GetByIdAsync(page.Id, id);
        if (currentBlock is null) return NotFound();
        var result = await _blockService.UpdateAsync(page.Id, id, new UpdatePageContentBlockRequest
        {
            BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = model.VideoUrl,
            IsActive = currentBlock.IsActive,
            EnforceExclusiveActivation = true,
            Translations = await BuildTranslationsAsync(model)
        });

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        if (currentBlock.IsActive) await UpdateEyebrowAsync(page, model.Eyebrow);
        TempData["SuccessMessage"] = "Kayıt güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, bool isActive)
    {
        var page = await FindManagedPageAsync();
        if (page is null) return NotFound();
        var result = await _blockService.SetActiveAsync(page.Id, id, isActive);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded
                ? (isActive ? "Seçilen kayıt aktif edildi; diğer kayıtlar pasife alındı." : "Kayıt pasife alındı.")
                : result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var page = await FindManagedPageAsync();
        if (page is null) return NotFound();
        var result = await _blockService.DeleteAsync(page.Id, id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Kayıt silindi." : result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    private async Task<IReadOnlyList<PageContentBlockTranslationInput>> BuildTranslationsAsync(RecordingFormViewModel model)
    {
        var languages = await _pageService.GetActiveLanguagesAsync();
        return languages.Select(language => new PageContentBlockTranslationInput
        {
            LanguageId = language.Id,
            Title = language.Code == "TR" ? model.Title : null,
            Content = language.Code == "TR" ? model.Description : null
        }).ToList();
    }

    private async Task<PageDto?> EnsureManagedPageAsync(string? eyebrow)
    {
        var page = await FindManagedPageAsync();
        if (page is not null) return page;
        var languages = await _pageService.GetActiveLanguagesAsync();
        var result = await _pageService.CreateAsync(new CreatePageRequest
        {
            Translations = languages.Select(language => new PageTranslationInput
            {
                LanguageId = language.Id,
                Title = language.Code == "TR" ? ManagedPageTitle : null,
                SeoUrl = language.Code == "TR" ? ManagedPageSeoUrl : null,
                MetaTitle = language.Code == "TR" ? eyebrow : null
            }).ToList()
        });
        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return null;
        }
        return await FindManagedPageAsync();
    }

    private async Task UpdateEyebrowAsync(PageDto page, string? eyebrow)
    {
        await _pageService.UpdateAsync(page.Id, new UpdatePageRequest
        {
            Translations = page.Translations.Select(translation => new PageTranslationInput
            {
                LanguageId = translation.LanguageId,
                Title = translation.LanguageCode == "TR" ? ManagedPageTitle : translation.Title,
                SeoUrl = translation.LanguageCode == "TR" ? ManagedPageSeoUrl : translation.SeoUrl,
                MetaTitle = translation.LanguageCode == "TR" ? eyebrow : translation.MetaTitle,
                MetaDescription = translation.MetaDescription
            }).ToList()
        });
    }

    private async Task<PageDto?> FindManagedPageAsync() =>
        (await _pageService.GetAllAsync()).FirstOrDefault(page =>
            page.Translations.Any(translation =>
                string.Equals(translation.SeoUrl, ManagedPageSeoUrl, StringComparison.OrdinalIgnoreCase)));
}
