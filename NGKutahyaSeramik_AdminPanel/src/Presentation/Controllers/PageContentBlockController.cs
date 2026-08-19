using Application.Pages;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Page;

namespace Presentation.Controllers;

/// <summary>İçerik blokları Sayfa Yönetimi'nin bir parçasıdır — RBAC PageController.EditRoles ile
/// birebir aynı (Admin+İçerik Editörü). SEO Editörü'nün blok içeriğine hiç erişimi yok.</summary>
[Authorize(Roles = EditRoles)]
public class PageContentBlockController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly PageContentBlockService _pageContentBlockService;

    public PageContentBlockController(PageContentBlockService pageContentBlockService)
    {
        _pageContentBlockService = pageContentBlockService;
    }

    public async Task<IActionResult> Create(int pageId)
    {
        var model = new PageContentBlockFormViewModel
        {
            PageId = pageId,
            BlockTypeOptions = BuildBlockTypeOptions(null),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Create(PageContentBlockFormViewModel model, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            model.BlockTypeOptions = BuildBlockTypeOptions(model.BlockType);
            return View(model);
        }

        var hasImage = image is { Length: > 0 };
        await using var stream = hasImage ? image!.OpenReadStream() : null;

        var request = new AddPageContentBlockRequest
        {
            PageId = model.PageId,
            BlockType = model.BlockType,
            VideoEmbedUrl = model.VideoEmbedUrl,
            Translations = MapToTranslationInputs(model.Translations),
            ImageOriginalFileName = hasImage ? image!.FileName : null,
            ImageContentType = hasImage ? image!.ContentType : null,
            ImageLength = hasImage ? image!.Length : null,
            ImageContent = stream
        };

        var result = await _pageContentBlockService.AddAsync(request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.BlockTypeOptions = BuildBlockTypeOptions(model.BlockType);
            return View(model);
        }

        TempData["SuccessMessage"] = "İçerik bloğu eklendi.";
        return RedirectToAction("Edit", "Page", new { id = model.PageId });
    }

    public async Task<IActionResult> Edit(int pageId, int blockId)
    {
        var block = await _pageContentBlockService.GetByIdAsync(pageId, blockId);
        if (block is null)
        {
            TempData["ErrorMessage"] = "Blok bulunamadı.";
            return RedirectToAction("Edit", "Page", new { id = pageId });
        }

        var model = new PageContentBlockFormViewModel
        {
            PageId = pageId,
            Id = block.Id,
            BlockType = block.BlockType,
            VideoEmbedUrl = block.VideoEmbedUrl,
            ExistingImagePath = block.ImagePath,
            BlockTypeOptions = BuildBlockTypeOptions(block.BlockType),
            Translations = block.Translations.Select(t => new PageContentBlockTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Title = t.Title,
                Content = t.Content
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Edit(int pageId, int blockId, PageContentBlockFormViewModel model, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            model.Id = blockId;
            model.BlockTypeOptions = BuildBlockTypeOptions(model.BlockType);
            return View(model);
        }

        var hasImage = image is { Length: > 0 };
        await using var stream = hasImage ? image!.OpenReadStream() : null;

        var request = new UpdatePageContentBlockRequest
        {
            BlockType = model.BlockType,
            VideoEmbedUrl = model.VideoEmbedUrl,
            Translations = MapToTranslationInputs(model.Translations),
            ImageOriginalFileName = hasImage ? image!.FileName : null,
            ImageContentType = hasImage ? image!.ContentType : null,
            ImageLength = hasImage ? image!.Length : null,
            ImageContent = stream,
            RemoveImage = model.RemoveImage
        };

        var result = await _pageContentBlockService.UpdateAsync(pageId, blockId, request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = blockId;
            model.BlockTypeOptions = BuildBlockTypeOptions(model.BlockType);
            return View(model);
        }

        TempData["SuccessMessage"] = "İçerik bloğu güncellendi.";
        return RedirectToAction("Edit", "Page", new { id = pageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int pageId, int blockId)
    {
        var result = await _pageContentBlockService.MoveUpAsync(pageId, blockId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Page", new { id = pageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int pageId, int blockId)
    {
        var result = await _pageContentBlockService.MoveDownAsync(pageId, blockId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Page", new { id = pageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int pageId, int blockId)
    {
        var result = await _pageContentBlockService.DeleteAsync(pageId, blockId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Blok silindi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Page", new { id = pageId });
    }

    private static IReadOnlyList<PageContentBlockTranslationInput> MapToTranslationInputs(
        IEnumerable<PageContentBlockTranslationFieldViewModel> translations) =>
        translations.Select(t => new PageContentBlockTranslationInput
        {
            LanguageId = t.LanguageId,
            Title = t.Title,
            Content = t.Content
        }).ToList();

    private async Task<List<PageContentBlockTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _pageContentBlockService.GetActiveLanguagesAsync();

        return languages.Select(l => new PageContentBlockTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }

    private static List<SelectListItem> BuildBlockTypeOptions(PageBlockType? selected) =>
        Enum.GetValues<PageBlockType>()
            .Select(t => new SelectListItem(PageEnumDisplay.GetBlockTypeLabel(t), t.ToString(), selected == t))
            .ToList();
}

