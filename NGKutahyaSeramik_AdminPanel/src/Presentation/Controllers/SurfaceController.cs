using Application.Surfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Surface;
using Presentation.Models.Collection;
using Application.Collections;

namespace Presentation.Controllers;

[Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager)]
public class SurfaceController : Controller
{
    private readonly SurfaceService _service;
    public SurfaceController(SurfaceService service) => _service = service;

    public async Task<IActionResult> Index() => View(await _service.GetListAsync());

    public async Task<IActionResult> Create() => View(new SurfaceFormViewModel
    {
        DisplayOrder = (await _service.GetAllAsync()).Select(x => x.DisplayOrder).DefaultIfEmpty(0).Max() + 1,
        Translations = await BuildEmptyTranslationsAsync()
    });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SurfaceFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _service.CreateAsync(model.Name, model.ImagePath, Enum.GetValues<Domain.Enums.ProductBrand>(), model.DisplayOrder, MapTranslations(model.Translations, model.Name));
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, result.Error ?? "Yüzey oluşturulamadı."); return View(model); }
        TempData["SuccessMessage"] = "Yüzey oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return NotFound();
        var values = await _service.GetTranslationsAsync(id);
        var translations = await BuildEmptyTranslationsAsync();
        foreach (var translation in translations)
        {
            string? Get(string field) => values.FirstOrDefault(x => x.LanguageId == translation.LanguageId && x.FieldName == field)?.Value;
            translation.Name = Get(CollectionFields.Name);
            translation.Description = Get(CollectionFields.Description);
            translation.SeoUrl = Get(CollectionFields.SeoUrl);
            translation.MetaTitle = Get(CollectionFields.MetaTitle);
            translation.MetaDescription = Get(CollectionFields.MetaDescription);
        }
        return View(new SurfaceFormViewModel { Id = item.Id, Name = item.Name, ImagePath = item.ImagePath, DisplayOrder = item.DisplayOrder, Translations = translations });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SurfaceFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _service.UpdateAsync(id, model.Name, model.ImagePath, Enum.GetValues<Domain.Enums.ProductBrand>(), model.DisplayOrder, MapTranslations(model.Translations, model.Name));
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, result.Error ?? "Yüzey güncellenemedi."); return View(model); }
        TempData["SuccessMessage"] = "Yüzey güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _service.ToggleAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Yüzey durumu güncellendi." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Admin), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Yüzey silindi." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<CollectionTranslationFieldViewModel>> BuildEmptyTranslationsAsync() =>
        (await _service.GetActiveLanguagesAsync()).Select(x => new CollectionTranslationFieldViewModel { LanguageId = x.Id, LanguageCode = x.Code, LanguageName = x.Name }).ToList();

    private static IReadOnlyList<CollectionTranslationInput> MapTranslations(IEnumerable<CollectionTranslationFieldViewModel> values, string name) =>
        values.Select(x => new CollectionTranslationInput { LanguageId = x.LanguageId, Name = x.LanguageCode == "TR" ? name : x.Name, Description = x.Description, SeoUrl = x.SeoUrl, MetaTitle = x.MetaTitle, MetaDescription = x.MetaDescription }).ToList();
}
