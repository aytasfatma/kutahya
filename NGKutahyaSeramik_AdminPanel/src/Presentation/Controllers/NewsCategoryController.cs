using Application.News;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.NewsCategory;

namespace Presentation.Controllers;

/// <summary>
/// Madde 17.2, Haber Yönetimi'nin fonksiyonları arasında "kategori"yi sayıyor — ayrı bir RBAC satırı
/// yok, bu yüzden NewsController ile birebir aynı yetki matrisi kullanıldı (Madde 30 Blog/Haber satırı).
/// </summary>
[Authorize(Roles = NewsCategoryController.ViewRoles)]
public class NewsCategoryController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private readonly NewsCategoryService _newsCategoryService;

    public NewsCategoryController(NewsCategoryService newsCategoryService)
    {
        _newsCategoryService = newsCategoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _newsCategoryService.GetAllAsync();
        return View(categories);
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new NewsCategoryFormViewModel
        {
            DisplayOrder = await _newsCategoryService.GetNextDisplayOrderAsync(),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new CreateNewsCategoryRequest
        {
            DisplayOrder = model.DisplayOrder!.Value,
            Translations = MapToTranslationInputs(model.Translations)
        };

        NewsCategoryOperationResult result;
        try
        {
            result = await _newsCategoryService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = NewsCategoryOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Bülten kategorisi oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _newsCategoryService.GetByIdAsync(id);
        if (category is null)
        {
            TempData["ErrorMessage"] = "Bülten kategorisi bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new NewsCategoryFormViewModel
        {
            Id = category.Id,
            DisplayOrder = category.DisplayOrder,
            Translations = category.Translations.Select(t => new NewsCategoryTranslationFieldViewModel
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
    public async Task<IActionResult> Edit(int id, NewsCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var request = new UpdateNewsCategoryRequest
        {
            DisplayOrder = model.DisplayOrder!.Value,
            Translations = MapToTranslationInputs(model.Translations)
        };

        NewsCategoryOperationResult result;
        try
        {
            result = await _newsCategoryService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = NewsCategoryOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Bülten kategorisi güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _newsCategoryService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Bülten kategorisi durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _newsCategoryService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Bülten kategorisi silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<NewsCategoryTranslationInput> MapToTranslationInputs(
        IEnumerable<NewsCategoryTranslationFieldViewModel> translations) =>
        translations.Select(t => new NewsCategoryTranslationInput
        {
            LanguageId = t.LanguageId,
            Name = t.Name
        }).ToList();

    private async Task<List<NewsCategoryTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _newsCategoryService.GetActiveLanguagesAsync();

        return languages.Select(l => new NewsCategoryTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }
}



