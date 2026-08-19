using Application.ReferenceProjects;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.ReferenceProject;

namespace Presentation.Controllers;

/// <summary>
/// Madde 30 (Yetkilendirme) tablosu Referans Proje Yönetimi'ni içermiyor — ancak Madde 7.2, İçerik
/// Editörü'nün yetki kapsamını "Sayfa, blog, haber, banner, referans proje, ürün açıklamaları" olarak
/// açıkça listeliyor. Bu nedenle Blog/Haber/Banner ile birebir aynı satır (Admin=Tam, İçerik
/// Editörü=CRUD, SEO Editörü=—/salt-görüntüleme, Ürün Yöneticisi=—) örnek alındı — Ürün Yöneticisi'nin
/// yetki tanımı (Madde 7.2: "Ürün ekleme/düzenleme, Excel import, görsel yükleme, doküman
/// ilişkilendirme") referans projeyi kapsamıyor.
/// </summary>
[Authorize(Roles = ReferenceProjectController.ViewRoles)]
public class ReferenceProjectController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private readonly ReferenceProjectService _referenceProjectService;
    private readonly ReferenceProjectImageService _referenceProjectImageService;

    public ReferenceProjectController(
        ReferenceProjectService referenceProjectService,
        ReferenceProjectImageService referenceProjectImageService)
    {
        _referenceProjectService = referenceProjectService;
        _referenceProjectImageService = referenceProjectImageService;
    }

    public async Task<IActionResult> Index()
    {
        var referenceProjects = await _referenceProjectService.GetAllAsync();
        return View(referenceProjects);
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new ReferenceProjectFormViewModel
        {
            DisplayOrder = await _referenceProjectService.GetNextDisplayOrderAsync(),
            ProjectTypeOptions = BuildProjectTypeOptions(null),
            RegionOptions = BuildRegionOptions(null),
            BrandOptions = BuildBrandOptions(null),
            ProductOptions = await BuildProductOptionsAsync(),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReferenceProjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var request = new CreateReferenceProjectRequest
        {
            Location = model.Location,
            Region = model.Region!.Value,
            Brand = model.Brand!.Value,
            ProjectType = model.ProjectType,
            Architect = model.Architect,
            Year = model.ProjectDate?.Year,
            DisplayOrder = model.DisplayOrder!.Value,
            RelatedProductIds = model.SelectedProductIds,
            Translations = MapToTranslationInputs(model.Translations)
        };

        ReferenceProjectOperationResult result;
        try
        {
            result = await _referenceProjectService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = ReferenceProjectOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Referans proje oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var referenceProject = await _referenceProjectService.GetByIdAsync(id);
        if (referenceProject is null)
        {
            TempData["ErrorMessage"] = "Referans proje bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ReferenceProjectFormViewModel
        {
            Id = referenceProject.Id,
            Location = referenceProject.Location,
            Region = referenceProject.Region,
            Brand = referenceProject.Brand,
            ProjectType = referenceProject.ProjectType,
            Architect = referenceProject.Architect,
            ProjectDate = referenceProject.Year.HasValue ? new DateTime(referenceProject.Year.Value, 1, 1) : null,
            DisplayOrder = referenceProject.DisplayOrder,
            SelectedProductIds = referenceProject.RelatedProducts.Select(p => p.Id).ToList(),
            ProjectTypeOptions = BuildProjectTypeOptions(referenceProject.ProjectType),
            RegionOptions = BuildRegionOptions(referenceProject.Region),
            BrandOptions = BuildBrandOptions(referenceProject.Brand),
            ProductOptions = await BuildProductOptionsAsync(),
            Images = (await _referenceProjectImageService.GetByReferenceProjectIdAsync(referenceProject.Id)).ToList(),
            Translations = referenceProject.Translations.Select(t => new ReferenceProjectTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                ProjectName = t.ProjectName,
                Description = t.Description,
                SeoUrl = t.SeoUrl
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReferenceProjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var request = new UpdateReferenceProjectRequest
        {
            Location = model.Location,
            Region = model.Region!.Value,
            Brand = model.Brand!.Value,
            ProjectType = model.ProjectType,
            Architect = model.Architect,
            Year = model.ProjectDate?.Year,
            DisplayOrder = model.DisplayOrder!.Value,
            RelatedProductIds = model.SelectedProductIds,
            Translations = MapToTranslationInputs(model.Translations)
        };

        ReferenceProjectOperationResult result;
        try
        {
            result = await _referenceProjectService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = ReferenceProjectOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Referans proje güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _referenceProjectService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Referans proje durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _referenceProjectService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Referans proje silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<ReferenceProjectTranslationInput> MapToTranslationInputs(
        IEnumerable<ReferenceProjectTranslationFieldViewModel> translations) =>
        translations.Select(t => new ReferenceProjectTranslationInput
        {
            LanguageId = t.LanguageId,
            ProjectName = t.ProjectName,
            Description = t.Description,
            SeoUrl = t.SeoUrl
        }).ToList();

    private async Task<List<ReferenceProjectTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _referenceProjectService.GetActiveLanguagesAsync();

        return languages.Select(l => new ReferenceProjectTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }

    private async Task RepopulateOptionsAsync(ReferenceProjectFormViewModel model)
    {
        model.ProjectTypeOptions = BuildProjectTypeOptions(model.ProjectType);
        model.RegionOptions = BuildRegionOptions(model.Region);
        model.BrandOptions = BuildBrandOptions(model.Brand);
        model.ProductOptions = await BuildProductOptionsAsync();
    }

    private async Task<List<SelectListItem>> BuildProductOptionsAsync()
    {
        var products = await _referenceProjectService.GetProductOptionsAsync();
        return products.Select(p => new SelectListItem(p.Label, p.Id.ToString())).ToList();
    }

    private static List<SelectListItem> BuildProjectTypeOptions(ReferenceProjectType? selected) =>
        Enum.GetValues<ReferenceProjectType>()
            .Select(t => new SelectListItem(ReferenceProjectEnumDisplay.GetProjectTypeLabel(t), t.ToString(), selected == t))
            .ToList();

    private static List<SelectListItem> BuildRegionOptions(ReferenceProjectRegion? selected) =>
        Enum.GetValues<ReferenceProjectRegion>()
            .Select(value => new SelectListItem(ReferenceProjectEnumDisplay.GetRegionLabel(value), value.ToString(), selected == value))
            .ToList();

    private static List<SelectListItem> BuildBrandOptions(ProductBrand? selected) =>
        Enum.GetValues<ProductBrand>()
            .Select(value => new SelectListItem(ReferenceProjectEnumDisplay.GetBrandLabel(value), value.ToString(), selected == value))
            .ToList();
}



