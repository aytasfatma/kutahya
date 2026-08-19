using Application.Blogs;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.BlogCategory;

namespace Presentation.Controllers;

/// <summary>
/// Madde 17.2, Blog Yönetimi'nin fonksiyonları arasında "kategori"yi sayıyor — ayrı bir RBAC satırı
/// yok, bu yüzden BlogController ile birebir aynı yetki matrisi kullanıldı (Madde 30 Blog/Haber satırı).
/// </summary>
[Authorize(Roles = BlogCategoryController.ViewRoles)]
public class BlogCategoryController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private readonly BlogCategoryService _blogCategoryService;

    public BlogCategoryController(BlogCategoryService blogCategoryService)
    {
        _blogCategoryService = blogCategoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _blogCategoryService.GetAllAsync();
        return View(categories);
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new BlogCategoryFormViewModel
        {
            DisplayOrder = await _blogCategoryService.GetNextDisplayOrderAsync(),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new CreateBlogCategoryRequest
        {
            DisplayOrder = model.DisplayOrder!.Value,
            Translations = MapToTranslationInputs(model.Translations)
        };

        BlogCategoryOperationResult result;
        try
        {
            result = await _blogCategoryService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = BlogCategoryOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Blog kategorisi oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _blogCategoryService.GetByIdAsync(id);
        if (category is null)
        {
            TempData["ErrorMessage"] = "Blog kategorisi bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new BlogCategoryFormViewModel
        {
            Id = category.Id,
            DisplayOrder = category.DisplayOrder,
            Translations = category.Translations.Select(t => new BlogCategoryTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Name = t.Name
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var request = new UpdateBlogCategoryRequest
        {
            DisplayOrder = model.DisplayOrder!.Value,
            Translations = MapToTranslationInputs(model.Translations)
        };

        BlogCategoryOperationResult result;
        try
        {
            result = await _blogCategoryService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = BlogCategoryOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Blog kategorisi güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _blogCategoryService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Blog kategorisi durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _blogCategoryService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Blog kategorisi silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<BlogCategoryTranslationInput> MapToTranslationInputs(
        IEnumerable<BlogCategoryTranslationFieldViewModel> translations) =>
        translations.Select(t => new BlogCategoryTranslationInput
        {
            LanguageId = t.LanguageId,
            Name = t.Name
        }).ToList();

    private async Task<List<BlogCategoryTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _blogCategoryService.GetActiveLanguagesAsync();

        return languages.Select(l => new BlogCategoryTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }
}



