using Application.Categories;
using Application.Collections;
using Application.ProductImages;
using Application.Products;
using Application.Surfaces;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Product;

namespace Presentation.Controllers;

[Authorize(Roles = ProductController.ViewRoles)]
public class ProductController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager + "," +
        ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    // Backlog #23 — Edit'te alan-seviyeli RBAC: İçerik Editörü (Name/ShortDescription/LongDescription)
    // ve SEO Editörü (SeoUrl/MetaTitle/MetaDescription) artık Edit action'ına erişebiliyor, ama hangi
    // alanları değiştirebilecekleri RestrictFieldsForRole ile POST'ta zorunlu olarak sınırlanıyor.
    // Create bilinçli olarak EditRoles'ta kalıyor — bu iki rol yeni ürün OLUŞTURAMAZ, yalnızca mevcut
    // ürünün izinli alanlarını düzenleyebilir.
    private const string FieldEditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager + "," +
        ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;
    private const int DefaultPageSize = 25;
    private const long MaxUploadBytes = 5 * 1024 * 1024;
    private const int MaxInitialImageCount = ProductImageService.MaxImagesPerProduct;

    private readonly ProductService _productService;
    private readonly CategoryService _categoryService;
    private readonly CollectionService _collectionService;
    private readonly ProductImageService _productImageService;
    private readonly SurfaceService _surfaceService;

    public ProductController(
        ProductService productService,
        CategoryService categoryService,
        CollectionService collectionService,
        ProductImageService productImageService,
        SurfaceService surfaceService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _collectionService = collectionService;
        _productImageService = productImageService;
        _surfaceService = surfaceService;
    }

    public async Task<IActionResult> Index(int page = 1, string? sort = null)
    {
        var selectedSort = NormalizeSort(sort);
        var result = await _productService.GetPagedAsync(new ProductQuery
        {
            PageNumber = page,
            PageSize = DefaultPageSize,
            Sort = selectedSort
        });

        return View(new ProductIndexViewModel
        {
            Page = result,
            Sort = selectedSort,
            SortOptions = BuildProductSortOptions(selectedSort)
        });
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel
        {
            Brands = [ProductBrand.NgSeramik],
            DisplayOrder = await _productService.GetNextDisplayOrderAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync(null),
            CollectionOptions = await BuildCollectionOptionsAsync(null),
            SurfaceOptions = await BuildSurfaceOptionsAsync(null),
            BrandOptions = BuildBrandOptions(null),
            StatusOptions = BuildStatusOptions(null),
            TechnicalOptions = await _productService.GetTechnicalOptionsAsync(),
            ImageTypeOptions = BuildImageTypeOptions(),
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit((MaxUploadBytes * MaxInitialImageCount) + 1024 * 1024)]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        await ResolveSurfaceAsync(model);
        await ValidateInitialImagesAsync(model);

        if (!ModelState.IsValid)
        {
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var request = MapToCreateRequest(model);
        ProductOperationResult result;
        try
        {
            result = await _productService.CreateAsync(request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = ProductOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            await RepopulateOptionsAsync(model);
            return View(model);
        }

        var imageErrors = await SaveInitialImagesAsync(result.EntityId!.Value, model);
        if (imageErrors.Count == 0)
        {
            TempData["SuccessMessage"] = model.InitialImageFiles.Any(file => file.Length > 0)
                ? "Urun olusturuldu ve gorseller eklendi."
                : "Urun basariyla olusturuldu.";
        }
        else
        {
            TempData.Remove("SuccessMessage");
            TempData["ErrorMessage"] = $"Urun olusturuldu ancak bazi gorseller eklenemedi: {string.Join(" ", imageErrors)}";
        }
        return RedirectToAction(nameof(Edit), new { id = result.EntityId });
    }

    [Authorize(Roles = FieldEditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            TempData["ErrorMessage"] = "Ürün bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            CategoryId = product.CategoryId,
            CollectionId = product.CollectionId,
            Brand = product.Brand,
            Brands = product.Brands.ToList(),
            Status = product.Status,
            ProductGroup = product.ProductGroup,
            Size = product.Size,
            Unit = product.Unit,
            Surface = product.Surface,
            SurfaceId = product.SurfaceId,
            Relief = product.Relief,
            SpecialSurface = product.SpecialSurface,
            FaceCount = product.FaceCount,
            HasFace = product.HasFace,
            HasVenue = product.HasVenue,
            Thickness = product.Thickness,
            BodyType = product.BodyType,
            Color = product.Color,
            ColorMaterial = product.ColorMaterial,
            ApplicationArea = product.ApplicationArea,
            UsageArea = product.UsageArea,
            Finish = product.Finish,
            PEI = product.PEI,
            VValue = product.VValue,
            RValue = product.RValue,
            DeepAbrasion = product.DeepAbrasion,
            HeatResistance = product.HeatResistance,
            AntiSlip = product.AntiSlip,
            GlazedGranite = product.GlazedGranite,
            BoxM2 = product.BoxM2,
            PalletM2 = product.PalletM2,
            DisplayOrder = product.DisplayOrder,
            CategoryOptions = await BuildCategoryOptionsAsync(product.CategoryId),
            CollectionOptions = await BuildCollectionOptionsAsync(product.CollectionId),
            SurfaceOptions = await BuildSurfaceOptionsAsync(product.SurfaceId),
            BrandOptions = BuildBrandOptions(product.Brand),
            StatusOptions = BuildStatusOptions(product.Status),
            TechnicalOptions = await _productService.GetTechnicalOptionsAsync(),
            Images = (await _productImageService.GetByProductIdAsync(product.Id)).ToList(),
            ImageTypeOptions = BuildImageTypeOptions(),
            Translations = product.Translations.Select(t => new ProductTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Name = t.Name,
                ShortDescription = t.ShortDescription,
                LongDescription = t.LongDescription,
                SeoUrl = t.SeoUrl,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription
            }).ToList()
        };

        ApplyFieldPermissions(model);

        return View(model);
    }

    [Authorize(Roles = FieldEditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        var current = await _productService.GetByIdAsync(id);
        if (current is null)
        {
            TempData["ErrorMessage"] = "Ürün bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        ApplyFieldPermissions(model);
        RestrictToPermittedFields(model, current);
        await ResolveSurfaceAsync(model);

        if (!ModelState.IsValid)
        {
            model.Id = id;
            await RepopulateOptionsAsync(model);
            await RepopulateImagesAsync(model, id);
            return View(model);
        }

        var request = MapToUpdateRequest(model);
        ProductOperationResult result;
        try
        {
            result = await _productService.UpdateAsync(id, request);
        }
        catch (Application.Common.SortOrderConflictException)
        {
            result = ProductOperationResult.Failure(Application.Common.SortOrderValidationMessages.Duplicate);
        }

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            await RepopulateOptionsAsync(model);
            await RepopulateImagesAsync(model, id);
            return View(model);
        }

        TempData["SuccessMessage"] = "Ürün güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Ürün silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private static CreateProductRequest MapToCreateRequest(ProductFormViewModel model) => new()
    {
        ProductCode = string.IsNullOrWhiteSpace(model.ProductCode) ? $"AUTO-{Guid.NewGuid():N}" : model.ProductCode,
        CategoryId = model.CategoryId,
        CollectionId = model.CollectionId,
        SurfaceId = model.SurfaceId,
        Brand = model.Brand,
        Brands = model.Brands,
        Status = model.Status,
        ProductGroup = model.ProductGroup,
        Size = model.Size ?? string.Empty,
        Unit = model.Unit ?? string.Empty,
        Surface = model.Surface,
        Relief = model.Relief,
        SpecialSurface = model.SpecialSurface,
        FaceCount = model.FaceCount,
        HasFace = model.HasFace,
        HasVenue = model.HasVenue,
        Thickness = model.Thickness,
        BodyType = model.BodyType,
        Color = model.Color,
        ColorMaterial = model.ColorMaterial,
        ApplicationArea = model.ApplicationArea,
        UsageArea = model.UsageArea,
        Finish = model.Finish,
        PEI = model.PEI,
        VValue = model.VValue,
        RValue = model.RValue,
        DeepAbrasion = model.DeepAbrasion,
        HeatResistance = model.HeatResistance,
        AntiSlip = model.AntiSlip,
        GlazedGranite = model.GlazedGranite,
        BoxM2 = model.BoxM2,
        PalletM2 = model.PalletM2,
        DisplayOrder = model.DisplayOrder ?? 0,
        Translations = MapToTranslationInputs(model.Translations)
    };

    private static UpdateProductRequest MapToUpdateRequest(ProductFormViewModel model) => new()
    {
        ProductCode = string.IsNullOrWhiteSpace(model.ProductCode) ? $"AUTO-{Guid.NewGuid():N}" : model.ProductCode,
        CategoryId = model.CategoryId,
        CollectionId = model.CollectionId,
        SurfaceId = model.SurfaceId,
        Brand = model.Brand,
        Brands = model.Brands,
        Status = model.Status,
        ProductGroup = model.ProductGroup,
        Size = model.Size ?? string.Empty,
        Unit = model.Unit ?? string.Empty,
        Surface = model.Surface,
        Relief = model.Relief,
        SpecialSurface = model.SpecialSurface,
        FaceCount = model.FaceCount,
        HasFace = model.HasFace,
        HasVenue = model.HasVenue,
        Thickness = model.Thickness,
        BodyType = model.BodyType,
        Color = model.Color,
        ColorMaterial = model.ColorMaterial,
        ApplicationArea = model.ApplicationArea,
        UsageArea = model.UsageArea,
        Finish = model.Finish,
        PEI = model.PEI,
        VValue = model.VValue,
        RValue = model.RValue,
        DeepAbrasion = model.DeepAbrasion,
        HeatResistance = model.HeatResistance,
        AntiSlip = model.AntiSlip,
        GlazedGranite = model.GlazedGranite,
        BoxM2 = model.BoxM2,
        PalletM2 = model.PalletM2,
        DisplayOrder = model.DisplayOrder ?? 0,
        Translations = MapToTranslationInputs(model.Translations)
    };

    private static IReadOnlyList<ProductTranslationInput> MapToTranslationInputs(
        IEnumerable<ProductTranslationFieldViewModel> translations) =>
        translations.Select(t => new ProductTranslationInput
        {
            LanguageId = t.LanguageId,
            Name = t.Name,
            ShortDescription = t.ShortDescription,
            LongDescription = t.LongDescription,
            SeoUrl = t.SeoUrl,
            MetaTitle = t.MetaTitle,
            MetaDescription = t.MetaDescription
        }).ToList();

    private async Task<List<ProductTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _productService.GetActiveLanguagesAsync();

        return languages.Select(l => new ProductTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }

    /// <summary>Backlog #23 — View'da hangi alan gruplarının disabled render edileceğini belirler.
    /// Admin/Ürün Yöneticisi kısıtlanmaz; İçerik Editörü yalnızca içerik alanlarına, SEO Editörü
    /// yalnızca SEO alanlarına erişebilir (native alanlara ikisi de erişemez).</summary>
    private void ApplyFieldPermissions(ProductFormViewModel model)
    {
        if (User.IsInRole(ApplicationRoles.Admin) || User.IsInRole(ApplicationRoles.ProductManager))
        {
            return;
        }

        model.CanEditNativeFields = false;
        model.CanEditContentFields = User.IsInRole(ApplicationRoles.ContentEditor);
        model.CanEditSeoFields = User.IsInRole(ApplicationRoles.SeoEditor);
    }

    /// <summary>Backlog #23 — Overposting koruması: Admin/Ürün Yöneticisi dışındaki roller için,
    /// POST edilen değerlerden yalnızca rolün izinli olduğu Translation alanları kabul edilir;
    /// izinsiz her alan (tüm native alanlar dahil) DB'den taze okunan mevcut değerle geri yazılır.
    /// Yalnızca View'da input'ları disabled yapmak yeterli değildir (form dışından/curl ile POST
    /// edilen değerler bu metod olmadan kabul edilirdi) — bu yüzden Controller'da da zorunlu.</summary>
    private void RestrictToPermittedFields(ProductFormViewModel model, ProductDto current)
    {
        if (User.IsInRole(ApplicationRoles.Admin) || User.IsInRole(ApplicationRoles.ProductManager))
        {
            return;
        }

        model.ProductCode = current.ProductCode;
        model.CategoryId = current.CategoryId;
        model.CollectionId = current.CollectionId;
        model.Brand = current.Brand;
        model.Brands = current.Brands.ToList();
        model.Status = current.Status;
        model.ProductGroup = current.ProductGroup;
        model.Size = current.Size;
        model.Unit = current.Unit;
        model.Surface = current.Surface;
        model.Relief = current.Relief;
        model.SpecialSurface = current.SpecialSurface;
        model.FaceCount = current.FaceCount;
        model.HasFace = current.HasFace;
        model.HasVenue = current.HasVenue;
        model.Thickness = current.Thickness;
        model.BodyType = current.BodyType;
        model.Color = current.Color;
        model.ColorMaterial = current.ColorMaterial;
        model.ApplicationArea = current.ApplicationArea;
        model.UsageArea = current.UsageArea;
        model.Finish = current.Finish;
        model.PEI = current.PEI;
        model.VValue = current.VValue;
        model.RValue = current.RValue;
        model.DeepAbrasion = current.DeepAbrasion;
        model.HeatResistance = current.HeatResistance;
        model.AntiSlip = current.AntiSlip;
        model.GlazedGranite = current.GlazedGranite;
        model.BoxM2 = current.BoxM2;
        model.PalletM2 = current.PalletM2;
        model.DisplayOrder = current.DisplayOrder;

        IReadOnlyList<string> allowedFields = User.IsInRole(ApplicationRoles.ContentEditor)
            ? ProductFieldPermissions.ContentEditorFields
            : User.IsInRole(ApplicationRoles.SeoEditor)
                ? ProductFieldPermissions.SeoEditorFields
                : [];

        foreach (var translation in model.Translations)
        {
            var currentTranslation = current.Translations.FirstOrDefault(t => t.LanguageId == translation.LanguageId);
            if (currentTranslation is null)
            {
                continue;
            }

            if (!allowedFields.Contains(ProductFields.Name))
            {
                translation.Name = currentTranslation.Name;
            }

            if (!allowedFields.Contains(ProductFields.ShortDescription))
            {
                translation.ShortDescription = currentTranslation.ShortDescription;
            }

            if (!allowedFields.Contains(ProductFields.LongDescription))
            {
                translation.LongDescription = currentTranslation.LongDescription;
            }

            if (!allowedFields.Contains(ProductFields.SeoUrl))
            {
                translation.SeoUrl = currentTranslation.SeoUrl;
            }

            if (!allowedFields.Contains(ProductFields.MetaTitle))
            {
                translation.MetaTitle = currentTranslation.MetaTitle;
            }

            if (!allowedFields.Contains(ProductFields.MetaDescription))
            {
                translation.MetaDescription = currentTranslation.MetaDescription;
            }
        }
    }

    private async Task ValidateInitialImagesAsync(ProductFormViewModel model)
    {
        var files = model.InitialImageFiles.Where(file => file.Length > 0).ToList();
        if (files.Count > MaxInitialImageCount)
        {
            ModelState.AddModelError(nameof(model.InitialImageFiles), $"En fazla {MaxInitialImageCount} gorsel secilebilir.");
            return;
        }

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var result = await _productImageService.ValidateUploadAsync(new AddProductImageRequest
            {
                ProductId = 0,
                ImageType = model.InitialImageType,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Content = stream
            });

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(model.InitialImageFiles), $"{file.FileName}: {result.ErrorMessage}");
            }
        }
    }

    private async Task<List<string>> SaveInitialImagesAsync(int productId, ProductFormViewModel model)
    {
        var errors = new List<string>();
        foreach (var file in model.InitialImageFiles.Where(file => file.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            var result = await _productImageService.AddImageAsync(new AddProductImageRequest
            {
                ProductId = productId,
                ImageType = model.InitialImageType,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Content = stream
            });

            if (!result.Succeeded)
            {
                errors.Add($"{file.FileName}: {result.ErrorMessage}");
            }
        }

        return errors;
    }

    private async Task RepopulateOptionsAsync(ProductFormViewModel model)
    {
        model.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
        model.CollectionOptions = await BuildCollectionOptionsAsync(model.CollectionId);
        model.SurfaceOptions = await BuildSurfaceOptionsAsync(model.SurfaceId);
        model.BrandOptions = BuildBrandOptions(model.Brand);
        model.StatusOptions = BuildStatusOptions(model.Status);
        model.TechnicalOptions = await _productService.GetTechnicalOptionsAsync();
        model.ImageTypeOptions = BuildImageTypeOptions();
    }

    private async Task RepopulateImagesAsync(ProductFormViewModel model, int productId)
    {
        model.Images = (await _productImageService.GetByProductIdAsync(productId)).ToList();
        model.ImageTypeOptions = BuildImageTypeOptions();
    }

    private async Task<List<SelectListItem>> BuildCategoryOptionsAsync(int? selectedId)
    {
        var categories = await _categoryService.GetOptionItemsAsync();

        var roots = categories
            .Where(c => c.ParentCategoryId is null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.DisplayName);

        var options = new List<SelectListItem>();

        foreach (var root in roots)
        {
            options.Add(new SelectListItem(root.DisplayName ?? $"#{root.Id}", root.Id.ToString(), selectedId == root.Id));

            var children = categories
                .Where(c => c.ParentCategoryId == root.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.DisplayName);

            foreach (var child in children)
            {
                options.Add(new SelectListItem(
                    $"— {child.DisplayName ?? $"#{child.Id}"}",
                    child.Id.ToString(),
                    selectedId == child.Id));
            }
        }

        return options;
    }

    private async Task<List<SelectListItem>> BuildCollectionOptionsAsync(int? selectedId)
    {
        var collections = await _collectionService.GetOptionItemsAsync();

        return collections
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.DisplayName)
            .Select(c => new SelectListItem(c.DisplayName ?? $"#{c.Id}", c.Id.ToString(), selectedId == c.Id))
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildSurfaceOptionsAsync(int? selectedId)
    {
        return (await _surfaceService.GetAllAsync())
            .Where(x => x.IsActive || x.Id == selectedId)
            .OrderBy(x => x.Name, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true))
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == selectedId))
            .ToList();
    }

    private async Task ResolveSurfaceAsync(ProductFormViewModel model)
    {
        if (!model.SurfaceId.HasValue)
        {
            model.Surface = null;
            return;
        }

        var surface = await _surfaceService.GetByIdAsync(model.SurfaceId.Value);
        if (surface is null)
            ModelState.AddModelError(nameof(model.SurfaceId), "Seçilen yüzey bulunamadı.");
        else
            model.Surface = surface.Name;
    }

    private static List<SelectListItem> BuildBrandOptions(ProductBrand? selected) =>
        Enum.GetValues<ProductBrand>()
            .Select(b => new SelectListItem(ProductEnumDisplay.GetBrandLabel(b), b.ToString(), selected == b))
            .ToList();

    private static List<SelectListItem> BuildStatusOptions(ProductStatus? selected) =>
        Enum.GetValues<ProductStatus>()
            .Select(s => new SelectListItem(ProductEnumDisplay.GetStatusLabel(s), s.ToString(), selected == s))
            .ToList();

    private static List<SelectListItem> BuildImageTypeOptions() =>
        Enum.GetValues<ProductImageType>()
            .Select(t => new SelectListItem(ProductImageEnumDisplay.GetImageTypeLabel(t), t.ToString()))
            .ToList();

    private static string NormalizeSort(string? sort) =>
        sort switch
        {
            ProductSortOptions.ProductCode => ProductSortOptions.ProductCode,
            ProductSortOptions.Newest => ProductSortOptions.Newest,
            ProductSortOptions.NameAsc => ProductSortOptions.NameAsc,
            _ => ProductSortOptions.DisplayOrder
        };

    private static List<SelectListItem> BuildProductSortOptions(string selectedSort) =>
        new()
        {
            new SelectListItem("Sıralama", ProductSortOptions.DisplayOrder, selectedSort == ProductSortOptions.DisplayOrder),
            new SelectListItem("Ürün kodu", ProductSortOptions.ProductCode, selectedSort == ProductSortOptions.ProductCode),
            new SelectListItem("En yeni", ProductSortOptions.Newest, selectedSort == ProductSortOptions.Newest),
            new SelectListItem("Ürün adı (A-Z)", ProductSortOptions.NameAsc, selectedSort == ProductSortOptions.NameAsc)
        };
}



