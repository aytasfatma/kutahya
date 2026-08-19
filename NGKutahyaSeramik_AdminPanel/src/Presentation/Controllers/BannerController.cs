using Application.Banners;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Banner;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Yetkilendirme) Banner satırı: Admin=Tam, İçerik Editörü=CRUD, SEO Editörü=—,
/// Ürün Yöneticisi=— (Blog/Haber'den farklı — Banner'da SEO alanı hiç olmadığı için SEO Editörü'ne
/// salt-görüntüleme bile verilmedi; yalnızca Admin+İçerik Editörü'nün herhangi bir erişimi var).</summary>
[Authorize(Roles = BannerController.ViewRoles)]
public class BannerController : Controller
{
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    private const string EditRoles = ViewRoles;

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly BannerService _bannerService;

    public BannerController(BannerService bannerService)
    {
        _bannerService = bannerService;
    }

    public async Task<IActionResult> Index()
    {
        var banners = await _bannerService.GetAllAsync();
        return View(banners.OrderBy(b => b.DisplayOrder).ThenBy(b => b.DisplayName).ToList());
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new BannerFormViewModel
        {
            DisplayOrder = await _bannerService.GetNextDisplayOrderAsync(),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Create(BannerFormViewModel model, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var hasImage = image is { Length: > 0 };
        await using var stream = hasImage ? image!.OpenReadStream() : null;

        var request = new CreateBannerRequest
        {
            PublishStartDate = model.PublishStartDate,
            PublishEndDate = model.PublishEndDate,
            DisplayOrder = model.DisplayOrder!.Value,
            Translations = MapToTranslationInputs(model.Translations),
            ImageOriginalFileName = hasImage ? image!.FileName : null,
            ImageContentType = hasImage ? image!.ContentType : null,
            ImageLength = hasImage ? image!.Length : null,
            ImageContent = stream
        };

        BannerOperationResult result;
        try
        {
            result = await _bannerService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = BannerOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Banner oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var banner = await _bannerService.GetByIdAsync(id);
        if (banner is null)
        {
            TempData["ErrorMessage"] = "Banner bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new BannerFormViewModel
        {
            Id = banner.Id,
            PublishStartDate = banner.PublishStartDate,
            PublishEndDate = banner.PublishEndDate,
            DisplayOrder = banner.DisplayOrder,
            ExistingImagePath = banner.ImagePath,
            Translations = banner.Translations.Select(t => new BannerTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Title = t.Title,
                Subtitle = t.Subtitle,
                ButtonText = t.ButtonText,
                ButtonUrl = t.ButtonUrl
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Edit(int id, BannerFormViewModel model, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var hasImage = image is { Length: > 0 };
        await using var stream = hasImage ? image!.OpenReadStream() : null;

        var request = new UpdateBannerRequest
        {
            PublishStartDate = model.PublishStartDate,
            PublishEndDate = model.PublishEndDate,
            DisplayOrder = model.DisplayOrder!.Value,
            Translations = MapToTranslationInputs(model.Translations),
            ImageOriginalFileName = hasImage ? image!.FileName : null,
            ImageContentType = hasImage ? image!.ContentType : null,
            ImageLength = hasImage ? image!.Length : null,
            ImageContent = stream,
            RemoveImage = model.RemoveImage
        };

        BannerOperationResult result;
        try
        {
            result = await _bannerService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = BannerOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Banner güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _bannerService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Banner durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bannerService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Banner silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<BannerTranslationInput> MapToTranslationInputs(
        IEnumerable<BannerTranslationFieldViewModel> translations) =>
        translations.Select(t => new BannerTranslationInput
        {
            LanguageId = t.LanguageId,
            Title = t.Title,
            Subtitle = t.Subtitle,
            ButtonText = t.ButtonText,
            ButtonUrl = t.ButtonUrl
        }).ToList();

    private async Task<List<BannerTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _bannerService.GetActiveLanguagesAsync();

        return languages.Select(l => new BannerTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }
}



