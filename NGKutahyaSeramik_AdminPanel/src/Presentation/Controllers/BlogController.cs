using Application.Blogs;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Blog;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Yetkilendirme) Blog/Haber satırı: Admin=Tam, İçerik Editörü=CRUD, SEO Editörü=Meta Alanları
/// (bu projede alan-seviyeli RBAC hiç uygulanmadı — Task 5'ten beri bilinçli, tüm modüllerde tutarlı;
/// SEO Editörü burada da salt-görüntüleme ile sınırlı), Ürün Yöneticisi=—.</summary>
[Authorize(Roles = BlogController.ViewRoles)]
public class BlogController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly BlogService _blogService;

    public BlogController(BlogService blogService)
    {
        _blogService = blogService;
    }

    public async Task<IActionResult> Index()
    {
        var blogs = await _blogService.GetAllAsync();
        return View(blogs.OrderByDescending(b => b.PublishDate).ThenByDescending(b => b.Id).ToList());
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new BlogFormViewModel
        {
            Status = BlogStatus.Draft,
            BlogCategoryOptions = await BuildBlogCategoryOptionsAsync(null),
            StatusOptions = BuildStatusOptions(null),
            RelatedBlogOptions = await BuildRelatedBlogOptionsAsync(null, []),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit((MaxUploadBytes * 2) + 8192)]
    public async Task<IActionResult> Create(BlogFormViewModel model, IFormFile? featuredImage, IFormFile? secondaryImage)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var hasImage = featuredImage is { Length: > 0 };
        var hasSecondaryImage = secondaryImage is { Length: > 0 };
        await using var stream = hasImage ? featuredImage!.OpenReadStream() : null;
        await using var secondaryStream = hasSecondaryImage ? secondaryImage!.OpenReadStream() : null;

        var request = new CreateBlogRequest
        {
            BlogCategoryId = model.BlogCategoryId,
            Author = model.Author,
            PublishDate = model.PublishDate,
            Status = model.Status,
            IsTrend = model.IsTrend,
            TagNames = ParseTagNames(model.TagsInput),
            RelatedBlogIds = model.SelectedRelatedBlogIds,
            Translations = MapToTranslationInputs(model.Translations),
            FeaturedImageOriginalFileName = hasImage ? featuredImage!.FileName : null,
            FeaturedImageContentType = hasImage ? featuredImage!.ContentType : null,
            FeaturedImageLength = hasImage ? featuredImage!.Length : null,
            FeaturedImageContent = stream,
            SecondaryImageOriginalFileName = hasSecondaryImage ? secondaryImage!.FileName : null,
            SecondaryImageContentType = hasSecondaryImage ? secondaryImage!.ContentType : null,
            SecondaryImageLength = hasSecondaryImage ? secondaryImage!.Length : null,
            SecondaryImageContent = secondaryStream
        };

        var result = await _blogService.CreateAsync(request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Blog yazısı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var blog = await _blogService.GetByIdAsync(id);
        if (blog is null)
        {
            TempData["ErrorMessage"] = "Blog yazısı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new BlogFormViewModel
        {
            Id = blog.Id,
            BlogCategoryId = blog.BlogCategoryId,
            Author = blog.Author,
            PublishDate = blog.PublishDate,
            Status = blog.Status,
            IsTrend = blog.IsTrend,
            TagsInput = string.Join(", ", blog.Tags),
            SelectedRelatedBlogIds = blog.RelatedPosts.Select(p => p.Id).ToList(),
            ExistingFeaturedImagePath = blog.FeaturedImagePath,
            ExistingSecondaryImagePath = blog.SecondaryImagePath,
            BlogCategoryOptions = await BuildBlogCategoryOptionsAsync(blog.BlogCategoryId),
            StatusOptions = BuildStatusOptions(blog.Status),
            RelatedBlogOptions = await BuildRelatedBlogOptionsAsync(blog.Id, blog.RelatedPosts.Select(p => p.Id).ToList()),
            Translations = blog.Translations.Select(t => new BlogTranslationFieldViewModel
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
    [RequestSizeLimit((MaxUploadBytes * 2) + 8192)]
    public async Task<IActionResult> Edit(int id, BlogFormViewModel model, IFormFile? featuredImage, IFormFile? secondaryImage)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var hasImage = featuredImage is { Length: > 0 };
        var hasSecondaryImage = secondaryImage is { Length: > 0 };
        await using var stream = hasImage ? featuredImage!.OpenReadStream() : null;
        await using var secondaryStream = hasSecondaryImage ? secondaryImage!.OpenReadStream() : null;

        var request = new UpdateBlogRequest
        {
            BlogCategoryId = model.BlogCategoryId,
            Author = model.Author,
            PublishDate = model.PublishDate,
            Status = model.Status,
            IsTrend = model.IsTrend,
            TagNames = ParseTagNames(model.TagsInput),
            RelatedBlogIds = model.SelectedRelatedBlogIds,
            Translations = MapToTranslationInputs(model.Translations),
            FeaturedImageOriginalFileName = hasImage ? featuredImage!.FileName : null,
            FeaturedImageContentType = hasImage ? featuredImage!.ContentType : null,
            FeaturedImageLength = hasImage ? featuredImage!.Length : null,
            FeaturedImageContent = stream,
            RemoveFeaturedImage = model.RemoveFeaturedImage,
            SecondaryImageOriginalFileName = hasSecondaryImage ? secondaryImage!.FileName : null,
            SecondaryImageContentType = hasSecondaryImage ? secondaryImage!.ContentType : null,
            SecondaryImageLength = hasSecondaryImage ? secondaryImage!.Length : null,
            SecondaryImageContent = secondaryStream,
            RemoveSecondaryImage = model.RemoveSecondaryImage
        };

        var result = await _blogService.UpdateAsync(id, request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Blog yazısı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _blogService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Blog yazısı silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<string> ParseTagNames(string? tagsInput) =>
        (tagsInput ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static IReadOnlyList<BlogTranslationInput> MapToTranslationInputs(
        IEnumerable<BlogTranslationFieldViewModel> translations) =>
        translations.Select(t => new BlogTranslationInput
        {
            LanguageId = t.LanguageId,
            Title = t.Title,
            Excerpt = t.Excerpt,
            Content = t.Content,
            SeoUrl = t.SeoUrl,
            MetaTitle = t.MetaTitle,
            MetaDescription = t.MetaDescription
        }).ToList();

    private async Task<List<BlogTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _blogService.GetActiveLanguagesAsync();

        return languages.Select(l => new BlogTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }

    private async Task RepopulateOptionsAsync(BlogFormViewModel model)
    {
        model.BlogCategoryOptions = await BuildBlogCategoryOptionsAsync(model.BlogCategoryId);
        model.StatusOptions = BuildStatusOptions(model.Status);
        model.RelatedBlogOptions = await BuildRelatedBlogOptionsAsync(model.Id, model.SelectedRelatedBlogIds);
    }

    private async Task<List<SelectListItem>> BuildRelatedBlogOptionsAsync(int? excludeId, IReadOnlyList<int> selectedIds)
    {
        var options = await _blogService.GetBlogOptionsAsync(excludeId);
        return options
            .Select(o => new SelectListItem(o.Label, o.Id.ToString(), selectedIds.Contains(o.Id)))
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildBlogCategoryOptionsAsync(int? selectedId)
    {
        var categories = await _blogService.GetBlogCategoryOptionsAsync();
        var options = categories
            .Select(c => new SelectListItem(c.Label, c.Id.ToString(), selectedId == c.Id))
            .ToList();
        options.Add(new SelectListItem("Genel (-kategorisiz-)", string.Empty, selectedId is null));
        return options;
    }

    private static List<SelectListItem> BuildStatusOptions(BlogStatus? selected) =>
        Enum.GetValues<BlogStatus>()
            .Select(s => new SelectListItem(BlogEnumDisplay.GetStatusLabel(s), s.ToString(), selected == s))
            .ToList();
}

