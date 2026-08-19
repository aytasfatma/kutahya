using Application.News;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.News;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Yetkilendirme) Blog/Haber satırı: Admin=Tam, İçerik Editörü=CRUD, SEO Editörü=Meta Alanları
/// (bu projede alan-seviyeli RBAC hiç uygulanmadı — Task 5'ten beri bilinçli, tüm modüllerde tutarlı;
/// SEO Editörü burada da salt-görüntüleme ile sınırlı), Ürün Yöneticisi=—. BlogController ile birebir aynı matris.</summary>
[Authorize(Roles = NewsController.ViewRoles)]
public class NewsController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly NewsService _newsService;

    public NewsController(NewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<IActionResult> Index()
    {
        var newsItems = await _newsService.GetAllAsync();
        return View(newsItems.OrderByDescending(n => n.PublishDate).ThenByDescending(n => n.Id).ToList());
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new NewsFormViewModel
        {
            Status = NewsStatus.Draft,
            NewsCategoryOptions = await BuildNewsCategoryOptionsAsync(null),
            StatusOptions = BuildStatusOptions(null),
            RelatedNewsOptions = await BuildRelatedNewsOptionsAsync(null, []),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Create(NewsFormViewModel model, IFormFile? featuredImage)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var hasImage = featuredImage is { Length: > 0 };
        await using var stream = hasImage ? featuredImage!.OpenReadStream() : null;

        var request = new CreateNewsRequest
        {
            NewsCategoryId = model.NewsCategoryId,
            PublishDate = model.PublishDate,
            Status = model.Status,
            RelatedNewsIds = model.SelectedRelatedNewsIds,
            Translations = MapToTranslationInputs(model.Translations),
            FeaturedImageOriginalFileName = hasImage ? featuredImage!.FileName : null,
            FeaturedImageContentType = hasImage ? featuredImage!.ContentType : null,
            FeaturedImageLength = hasImage ? featuredImage!.Length : null,
            FeaturedImageContent = stream
        };

        var result = await _newsService.CreateAsync(request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Bülten oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var news = await _newsService.GetByIdAsync(id);
        if (news is null)
        {
            TempData["ErrorMessage"] = "Bülten bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new NewsFormViewModel
        {
            Id = news.Id,
            NewsCategoryId = news.NewsCategoryId,
            PublishDate = news.PublishDate,
            Status = news.Status,
            SelectedRelatedNewsIds = news.RelatedPosts.Select(p => p.Id).ToList(),
            ExistingFeaturedImagePath = news.FeaturedImagePath,
            NewsCategoryOptions = await BuildNewsCategoryOptionsAsync(news.NewsCategoryId),
            StatusOptions = BuildStatusOptions(news.Status),
            RelatedNewsOptions = await BuildRelatedNewsOptionsAsync(news.Id, news.RelatedPosts.Select(p => p.Id).ToList()),
            Translations = news.Translations.Select(t => new NewsTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Title = t.Title,
                Excerpt = t.Excerpt,
                Content = t.Content,
                SeoUrl = t.SeoUrl,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Edit(int id, NewsFormViewModel model, IFormFile? featuredImage)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var hasImage = featuredImage is { Length: > 0 };
        await using var stream = hasImage ? featuredImage!.OpenReadStream() : null;

        var request = new UpdateNewsRequest
        {
            NewsCategoryId = model.NewsCategoryId,
            PublishDate = model.PublishDate,
            Status = model.Status,
            RelatedNewsIds = model.SelectedRelatedNewsIds,
            Translations = MapToTranslationInputs(model.Translations),
            FeaturedImageOriginalFileName = hasImage ? featuredImage!.FileName : null,
            FeaturedImageContentType = hasImage ? featuredImage!.ContentType : null,
            FeaturedImageLength = hasImage ? featuredImage!.Length : null,
            FeaturedImageContent = stream,
            RemoveFeaturedImage = model.RemoveFeaturedImage
        };

        var result = await _newsService.UpdateAsync(id, request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Bülten güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _newsService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Bülten silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<NewsTranslationInput> MapToTranslationInputs(
        IEnumerable<NewsTranslationFieldViewModel> translations) =>
        translations.Select(t => new NewsTranslationInput
        {
            LanguageId = t.LanguageId,
            Title = t.Title,
            Excerpt = t.Excerpt,
            Content = t.Content,
            SeoUrl = t.SeoUrl,
            MetaTitle = t.MetaTitle,
            MetaDescription = t.MetaDescription
        }).ToList();

    private async Task<List<NewsTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _newsService.GetActiveLanguagesAsync();

        return languages.Select(l => new NewsTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }

    private async Task RepopulateOptionsAsync(NewsFormViewModel model)
    {
        model.NewsCategoryOptions = await BuildNewsCategoryOptionsAsync(model.NewsCategoryId);
        model.StatusOptions = BuildStatusOptions(model.Status);
        model.RelatedNewsOptions = await BuildRelatedNewsOptionsAsync(model.Id, model.SelectedRelatedNewsIds);
    }

    private async Task<List<SelectListItem>> BuildRelatedNewsOptionsAsync(int? excludeId, IReadOnlyList<int> selectedIds)
    {
        var options = await _newsService.GetNewsOptionsAsync(excludeId);
        return options
            .Select(o => new SelectListItem(o.Label, o.Id.ToString(), selectedIds.Contains(o.Id)))
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildNewsCategoryOptionsAsync(int? selectedId)
    {
        var categories = await _newsService.GetNewsCategoryOptionsAsync();
        return categories
            .Select((category, index) => new SelectListItem(
                category.Label,
                category.Id.ToString(),
                selectedId == category.Id || (selectedId is null && index == 0)))
            .ToList();
    }

    private static List<SelectListItem> BuildStatusOptions(NewsStatus? selected) =>
        Enum.GetValues<NewsStatus>()
            .Select(s => new SelectListItem(NewsEnumDisplay.GetStatusLabel(s), s.ToString(), selected == s))
            .ToList();
}

