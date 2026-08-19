using Application.Documents;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Document;

namespace Presentation.Controllers;

[Authorize(Roles = DocumentController.ViewRoles)]
public class DocumentController : Controller
{
    // Madde 30 (Yetkilendirme): Katalog/Doküman → Admin=Tam, İçerik Editörü=Yükleme, SEO Editörü=—, Ürün Yöneticisi=Tam.
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager;
    private const string CreateRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager + "," + ApplicationRoles.ContentEditor;

    private const long MaxUploadBytes = 20 * 1024 * 1024;

    private readonly DocumentService _documentService;

    public DocumentController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _documentService.GetAllAsync();
        return View(documents);
    }

    [Authorize(Roles = CreateRoles)]
    public async Task<IActionResult> Create()
    {
        var defaultLanguageId = (await _documentService.GetActiveLanguagesAsync()).FirstOrDefault()?.Id ?? 0;

        var model = new DocumentFormViewModel
        {
            LanguageId = defaultLanguageId,
            DisplayOrder = defaultLanguageId == 0
                ? 1
                : await _documentService.GetNextDisplayOrderAsync(default, defaultLanguageId),
            DocumentTypeOptions = BuildDocumentTypeOptions(null),
            BrandOptions = BuildBrandOptions(null),
            LanguageOptions = await BuildLanguageOptionsAsync(defaultLanguageId),
            ProductOptions = await BuildProductOptionsAsync(),
            CollectionOptions = await BuildCollectionOptionsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = CreateRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Create(DocumentFormViewModel model, IFormFile? file)
    {
        if (!ModelState.IsValid || file is null || file.Length == 0)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen bir PDF dosyası seçin.");
            }

            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var languageId = model.LanguageId != 0
            ? model.LanguageId
            : (await _documentService.GetActiveLanguagesAsync()).First().Id;
        var displayOrder = await _documentService.GetNextDisplayOrderAsync(model.DocumentType!.Value, languageId);

        await using var stream = file.OpenReadStream();
        var request = new CreateDocumentRequest
        {
            DocumentType = model.DocumentType!.Value,
            Brand = model.Brand!.Value,
            Title = model.Title,
            Description = model.Description,
            LanguageId = languageId,
            DisplayOrder = displayOrder,
            ProductIds = model.SelectedProductIds,
            CollectionIds = [model.SelectedCollectionId!.Value],
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = stream
        };

        DocumentOperationResult result;
        try
        {
            result = await _documentService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = DocumentOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Doküman yüklendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document is null)
        {
            TempData["ErrorMessage"] = "Doküman bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new DocumentFormViewModel
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            DocumentType = document.DocumentType,
            Brand = document.Brand,
            LanguageId = document.LanguageId,
            DisplayOrder = document.DisplayOrder,
            SelectedProductIds = document.RelatedProducts.Select(p => p.Id).ToList(),
            SelectedCollectionIds = document.RelatedCollections.Select(c => c.Id).ToList(),
            SelectedCollectionId = document.RelatedCollections.Select(c => (int?)c.Id).FirstOrDefault(),
            ExistingOriginalFileName = document.OriginalFileName,
            ExistingFilePath = document.FilePath,
            ExistingFileSizeLabel = document.FileSizeLabel,
            DocumentTypeOptions = BuildDocumentTypeOptions(document.DocumentType),
            BrandOptions = BuildBrandOptions(document.Brand),
            LanguageOptions = await BuildLanguageOptionsAsync(document.LanguageId),
            ProductOptions = await BuildProductOptionsAsync(),
            CollectionOptions = await BuildCollectionOptionsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Edit(int id, DocumentFormViewModel model, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var request = new UpdateDocumentRequest
        {
            DocumentType = model.DocumentType!.Value,
            Brand = model.Brand!.Value,
            Title = model.Title,
            Description = model.Description,
            LanguageId = model.LanguageId,
            DisplayOrder = model.DisplayOrder!.Value,
            ProductIds = model.SelectedProductIds,
            CollectionIds = [model.SelectedCollectionId!.Value],
            OriginalFileName = file is { Length: > 0 } ? file.FileName : null,
            ContentType = file is { Length: > 0 } ? file.ContentType : null,
            Length = file is { Length: > 0 } ? file.Length : null,
            Content = file is { Length: > 0 } ? file.OpenReadStream() : null
        };

        DocumentOperationResult result;
        try
        {
            result = await _documentService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = DocumentOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Doküman güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _documentService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Doküman durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _documentService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Doküman silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task RepopulateOptionsAsync(DocumentFormViewModel model)
    {
        model.DocumentTypeOptions = BuildDocumentTypeOptions(model.DocumentType);
        model.BrandOptions = BuildBrandOptions(model.Brand);
        model.LanguageOptions = await BuildLanguageOptionsAsync(model.LanguageId);
        model.ProductOptions = await BuildProductOptionsAsync();
        model.CollectionOptions = await BuildCollectionOptionsAsync();
    }

    private static readonly DocumentType[] SelectableDocumentTypes =
    [
        DocumentType.TechnicalSheet,
        DocumentType.Certificate
    ];

    private static List<SelectListItem> BuildDocumentTypeOptions(DocumentType? selected) =>
        SelectableDocumentTypes
            .Select(t => new SelectListItem(DocumentEnumDisplay.GetDocumentTypeLabel(t), t.ToString(), selected == t))
            .ToList();

    private static List<SelectListItem> BuildBrandOptions(ProductBrand? selected) =>
        Enum.GetValues<ProductBrand>()
            .Select(brand => new SelectListItem(
                Application.Products.ProductEnumDisplay.GetBrandLabel(brand), brand.ToString(), selected == brand))
            .ToList();

    private async Task<List<SelectListItem>> BuildLanguageOptionsAsync(int? selectedId)
    {
        var languages = await _documentService.GetActiveLanguagesAsync();
        return languages
            .Select(l => new SelectListItem($"{l.Name} ({l.Code})", l.Id.ToString(), selectedId == l.Id))
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildProductOptionsAsync()
    {
        var products = await _documentService.GetProductOptionsAsync();
        return products.Select(p => new SelectListItem(p.Label, p.Id.ToString())).ToList();
    }

    private async Task<List<SelectListItem>> BuildCollectionOptionsAsync()
    {
        var collections = await _documentService.GetCollectionOptionsAsync();
        return collections.Select(c => new SelectListItem(c.Label, c.Id.ToString())).ToList();
    }
}



