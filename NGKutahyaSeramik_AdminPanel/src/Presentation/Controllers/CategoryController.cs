using Application.Categories;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Category;

namespace Presentation.Controllers;

[Authorize(Roles = CategoryController.ViewRoles)]
public class CategoryController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager + "," +
        ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetTreeAsync();
        return View(categories);
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new CategoryFormViewModel
        {
            DisplayOrder = await _categoryService.GetNextDisplayOrderAsync(null),
            ParentCategoryOptions = await BuildParentOptionsAsync(null, null),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ParentCategoryOptions = await BuildParentOptionsAsync(model.ParentCategoryId, null);
            return View(model);
        }

        var request = new CreateCategoryRequest
        {
            ParentCategoryId = null,
            ImagePath = model.ImagePath,
            DisplayOrder = model.DisplayOrder!.Value,
            Brands = Enum.GetValues<Domain.Enums.ProductBrand>(),
            Translations = MapToTranslationInputs(model.Translations)
        };

        CategoryOperationResult result;
        try
        {
            result = await _categoryService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = CategoryOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.ParentCategoryOptions = await BuildParentOptionsAsync(model.ParentCategoryId, null);
            return View(model);
        }

        TempData["SuccessMessage"] = "Yüzey türü oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
        {
            TempData["ErrorMessage"] = "Yüzey türü bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            ParentCategoryId = category.ParentCategoryId,
            ImagePath = category.ImagePath,
            DisplayOrder = category.DisplayOrder,
            ParentCategoryOptions = await BuildParentOptionsAsync(category.ParentCategoryId, id),
            Translations = category.Translations.Select(t => new CategoryTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Name = t.Name,
                Description = t.Description,
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
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.ParentCategoryOptions = await BuildParentOptionsAsync(model.ParentCategoryId, id);
            return View(model);
        }

        var request = new UpdateCategoryRequest
        {
            ParentCategoryId = null,
            ImagePath = model.ImagePath,
            DisplayOrder = model.DisplayOrder!.Value,
            Brands = Enum.GetValues<Domain.Enums.ProductBrand>(),
            Translations = MapToTranslationInputs(model.Translations)
        };

        CategoryOperationResult result;
        try
        {
            result = await _categoryService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = CategoryOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            model.ParentCategoryOptions = await BuildParentOptionsAsync(model.ParentCategoryId, id);
            return View(model);
        }

        TempData["SuccessMessage"] = "Yüzey türü güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _categoryService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Yüzey türü durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Yüzey türü silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<CategoryTranslationInput> MapToTranslationInputs(
        IEnumerable<CategoryTranslationFieldViewModel> translations) =>
        translations.Select(t => new CategoryTranslationInput
        {
            LanguageId = t.LanguageId,
            Name = t.Name,
            Description = t.Description,
            SeoUrl = t.SeoUrl,
            MetaTitle = t.MetaTitle,
            MetaDescription = t.MetaDescription
        }).ToList();

    private async Task<List<CategoryTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _categoryService.GetActiveLanguagesAsync();

        return languages.Select(l => new CategoryTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }

    private async Task<List<SelectListItem>> BuildParentOptionsAsync(int? selectedId, int? excludeId)
    {
        var candidates = await _categoryService.GetParentCandidatesAsync();

        var options = new List<SelectListItem>
        {
            new("— (Ana Yüzey Türü) —", string.Empty, selectedId is null)
        };

        options.AddRange(candidates
            .Where(c => excludeId is null || c.Id != excludeId.Value)
            .Select(c => new SelectListItem(
                c.DisplayName ?? $"#{c.Id}",
                c.Id.ToString(),
                selectedId == c.Id)));

        return options;
    }
}



