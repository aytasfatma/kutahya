using Application.Collections;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Collection;

namespace Presentation.Controllers;

[Authorize(Roles = CollectionController.ViewRoles)]
public class CollectionController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager + "," +
        ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private readonly CollectionService _collectionService;

    public CollectionController(CollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    public async Task<IActionResult> Index()
    {
        var collections = await _collectionService.GetAllAsync();
        return View(collections);
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new CollectionFormViewModel
        {
            DisplayOrder = await _collectionService.GetNextDisplayOrderAsync(),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CollectionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new CreateCollectionRequest
        {
            ImagePath = model.ImagePath,
            DisplayOrder = model.DisplayOrder!.Value,
            Brands = Enum.GetValues<Domain.Enums.ProductBrand>(),
            Translations = MapToTranslationInputs(model.Translations)
        };

        CollectionOperationResult result;
        try
        {
            result = await _collectionService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = CollectionOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Koleksiyon oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var collection = await _collectionService.GetByIdAsync(id);
        if (collection is null)
        {
            TempData["ErrorMessage"] = "Koleksiyon bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CollectionFormViewModel
        {
            Id = collection.Id,
            ImagePath = collection.ImagePath,
            DisplayOrder = collection.DisplayOrder,
            Translations = collection.Translations.Select(t => new CollectionTranslationFieldViewModel
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
    public async Task<IActionResult> Edit(int id, CollectionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var request = new UpdateCollectionRequest
        {
            ImagePath = model.ImagePath,
            DisplayOrder = model.DisplayOrder!.Value,
            Brands = Enum.GetValues<Domain.Enums.ProductBrand>(),
            Translations = MapToTranslationInputs(model.Translations)
        };

        CollectionOperationResult result;
        try
        {
            result = await _collectionService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = CollectionOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Koleksiyon güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _collectionService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Koleksiyon durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _collectionService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Koleksiyon silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<CollectionTranslationInput> MapToTranslationInputs(
        IEnumerable<CollectionTranslationFieldViewModel> translations) =>
        translations.Select(t => new CollectionTranslationInput
        {
            LanguageId = t.LanguageId,
            Name = t.Name,
            Description = t.Description,
            SeoUrl = t.SeoUrl,
            MetaTitle = t.MetaTitle,
            MetaDescription = t.MetaDescription
        }).ToList();

    private async Task<List<CollectionTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _collectionService.GetActiveLanguagesAsync();

        return languages.Select(l => new CollectionTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }
}



