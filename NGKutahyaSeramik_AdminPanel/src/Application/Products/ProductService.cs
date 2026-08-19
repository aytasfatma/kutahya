using Application.Categories;
using Application.Common;
using Application.Collections;
using Application.ProductImages;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;

namespace Application.Products;

public class ProductService
{
    private const string TrLanguageCode = "TR";

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly ProductImageService _productImageService;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICollectionRepository collectionRepository,
        IProductImageRepository productImageRepository,
        ProductImageService productImageService,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _collectionRepository = collectionRepository;
        _productImageRepository = productImageRepository;
        _productImageService = productImageService;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        var result = new List<ProductDto>();
        foreach (var product in products)
        {
            result.Add(await MapToDtoAsync(product));
        }

        return result;
    }

    public async Task<IReadOnlyList<ProductSurfaceSummaryDto>> GetActiveSurfaceSummariesAsync(ProductBrand? brand = null)
    {
        var trCulture = new System.Globalization.CultureInfo("tr-TR");
        return (await _productRepository.GetAllAsync())
            .Where(product => product.Status == ProductStatus.Active && !string.IsNullOrWhiteSpace(product.Surface))
            .SelectMany(product => product.Brands
                .Where(productBrand => productBrand != ProductBrand.NgPerforma &&
                                       (!brand.HasValue || productBrand == brand.Value))
                .Select(productBrand => new { Product = product, Brand = productBrand }))
            .GroupBy(item => (item.Brand, Name: item.Product.Surface!.Trim().ToUpper(trCulture)))
            .Select(group => new ProductSurfaceSummaryDto(
                group.First().Product.Surface!.Trim(), group.Key.Brand, group.Count()))
            .OrderBy(item => item.Name, StringComparer.Create(trCulture, true))
            .ToList();
    }

    public Task<ProductPagedResult<ProductListItemDto>> GetPagedAsync(ProductQuery query) =>
        _productRepository.GetPagedListAsync(query);

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product is null ? null : await MapToDtoAsync(product);
    }

    public async Task<ProductDto?> GetBySeoUrlAsync(string seoUrl, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(seoUrl)) return null;
        var product = await _productRepository.GetBySeoUrlAsync(seoUrl, languageCode);
        return product is null ? null : await MapToDtoAsync(product);
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public Task<int> GetNextDisplayOrderAsync() =>
        _productRepository.GetNextDisplayOrderAsync();

    public Task<ProductTechnicalOptionsDto> GetTechnicalOptionsAsync() =>
        _productRepository.GetTechnicalOptionsAsync();

    public async Task<ProductOperationResult> CreateAsync(CreateProductRequest request)
    {
        var validation = await ValidateAsync(request, excludeProductId: null);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var product = new Product(
            request.ProductCode.Trim(),
            request.CategoryId,
            request.CollectionId,
            request.Brand,
            request.Status,
            NullIfBlank(request.CommercialName),
            NullIfBlank(request.ProductGroup),
            request.Size.Trim(),
            request.Unit.Trim(),
            NullIfBlank(request.Surface),
            NullIfBlank(request.Relief),
            NullIfBlank(request.SpecialSurface),
            request.FaceCount,
            request.Thickness,
            NullIfBlank(request.BodyType),
            NullIfBlank(request.Color),
            NullIfBlank(request.ColorMaterial),
            NullIfBlank(request.ApplicationArea),
            NullIfBlank(request.UsageArea),
            NullIfBlank(request.Finish),
            request.PEI,
            NullIfBlank(request.VValue),
            NullIfBlank(request.RValue),
            NullIfInvalidDeepAbrasion(request.DeepAbrasion),
            request.HeatResistance,
            request.AntiSlip,
            request.GlazedGranite,
            request.HasFace,
            request.HasVenue,
            request.BoxM2,
            request.PalletM2,
            request.DisplayOrder,
            EffectiveBrands(request));

        product.SetSurface(request.SurfaceId, request.Surface);

        await _productRepository.AddAsync(product);

        // Yeni Product'ın Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik,
        // gerçek FK/navigation değil — ADR-012), bu yüzden Create'te iki SaveChanges çağrısı gerekir.
        await _unitOfWork.SaveChangesAsync();

        await SaveTranslationsAsync(product.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return ProductOperationResult.Success(product.Id);
    }

    public async Task<ProductOperationResult> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return ProductOperationResult.Failure("Ürün bulunamadı.");
        }

        var validation = await ValidateAsync(request, excludeProductId: id);
        if (!validation.Succeeded)
        {
            return validation;
        }

        product.UpdateDetails(
            request.ProductCode.Trim(),
            request.CategoryId,
            request.CollectionId,
            request.Brand,
            request.Status,
            NullIfBlank(request.CommercialName),
            NullIfBlank(request.ProductGroup),
            request.Size.Trim(),
            request.Unit.Trim(),
            NullIfBlank(request.Surface),
            NullIfBlank(request.Relief),
            NullIfBlank(request.SpecialSurface),
            request.FaceCount,
            request.Thickness,
            NullIfBlank(request.BodyType),
            NullIfBlank(request.Color),
            NullIfBlank(request.ColorMaterial),
            NullIfBlank(request.ApplicationArea),
            NullIfBlank(request.UsageArea),
            NullIfBlank(request.Finish),
            request.PEI,
            NullIfBlank(request.VValue),
            NullIfBlank(request.RValue),
            NullIfInvalidDeepAbrasion(request.DeepAbrasion),
            request.HeatResistance,
            request.AntiSlip,
            request.GlazedGranite,
            request.HasFace,
            request.HasVenue,
            request.BoxM2,
            request.PalletM2,
            request.DisplayOrder,
            EffectiveBrands(request));

        product.SetSurface(request.SurfaceId, request.Surface);

        await SaveTranslationsAsync(product.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return ProductOperationResult.Success();
    }

    public async Task<ProductOperationResult> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return ProductOperationResult.Failure("Ürün bulunamadı.");
        }

        // Not: Doküman/Referans Proje ilişkisi bu task kapsamında değil (o entity'ler henüz yok) —
        // ilgili bağlılık kontrolleri ileride ilgili task'larda genişletilecek.
        // Görsel temizliği (DB kaydı + fiziksel dosya) Product silinmeden önce ProductImageService
        // üzerinden yapılır — DB cascade FK'si zaten kayıtları silecektir, ama fiziksel dosya
        // temizliğini garanti etmek için açık çağrı gerekir (Task 5.1).
        await _productImageService.DeleteAllForProductAsync(id);
        await _translationService.DeleteTranslationsForAsync(EntityType.Product, id);
        _productRepository.Remove(product);

        await _unitOfWork.SaveChangesAsync();
        return ProductOperationResult.Success();
    }

    private async Task<ProductOperationResult> ValidateAsync(ProductRequestBase request, int? excludeProductId)
    {
        var isDuplicateCode = await IsDuplicateProductCodeAsync(request.ProductCode.Trim(), excludeProductId);
        if (isDuplicateCode)
        {
            return ProductOperationResult.Failure("Bu ürün kodu başka bir üründe zaten kullanılıyor.");
        }

        if (request.CategoryId.HasValue &&
            await _categoryRepository.GetByIdAsync(request.CategoryId.Value) is null)
        {
            return ProductOperationResult.Failure("Seçilen kategori bulunamadı.");
        }

        if (await _collectionRepository.GetByIdAsync(request.CollectionId) is null)
        {
            return ProductOperationResult.Failure("Seçilen koleksiyon bulunamadı.");
        }

        if (EffectiveBrands(request).Count == 0)
        {
            return ProductOperationResult.Failure("En az bir marka seçilmelidir.");
        }

        if (request.Thickness is not null && request.Thickness <= 0)
        {
            return ProductOperationResult.Failure("Kalınlık sıfırdan büyük olmalıdır.");
        }

        if (ContainsMarkup(request.DeepAbrasion))
        {
            return ProductOperationResult.Failure("Derin aşınma değeri geçersiz karakter içeriyor.");
        }

        var allProducts = await _productRepository.GetAllAsync();
        if (request.DisplayOrder > 0 && SortOrderValidation.HasDuplicate(allProducts, request.DisplayOrder, excludeProductId, p => p.Id, p => p.DisplayOrder))
        {
            return ProductOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return ProductOperationResult.Failure("Aktif Türkçe (TR) dili bulunamadı.");
        }

        return ProductOperationResult.Success();
    }

    private async Task<bool> IsDuplicateProductCodeAsync(string productCode, int? excludeProductId)
    {
        var existing = await _productRepository.GetByProductCodeAsync(productCode);
        if (existing is null)
        {
            return false;
        }

        return !excludeProductId.HasValue || existing.Id != excludeProductId.Value;
    }

    private async Task SaveTranslationsAsync(int productId, IReadOnlyList<ProductTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(productId, input.LanguageId, ProductFields.Name, input.Name);
            await SaveFieldAsync(productId, input.LanguageId, ProductFields.ShortDescription, input.ShortDescription);
            await SaveFieldAsync(productId, input.LanguageId, ProductFields.LongDescription, input.LongDescription);
            await SaveFieldAsync(productId, input.LanguageId, ProductFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(productId, input.LanguageId, ProductFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(productId, input.LanguageId, ProductFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int productId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.Product, productId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.Product, productId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<ProductDto> MapToDtoAsync(Product product)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.Product, product.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new ProductTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Name = Get(ProductFields.Name),
                    ShortDescription = Get(ProductFields.ShortDescription),
                    LongDescription = Get(ProductFields.LongDescription),
                    SeoUrl = Get(ProductFields.SeoUrl),
                    MetaTitle = Get(ProductFields.MetaTitle),
                    MetaDescription = Get(ProductFields.MetaDescription)
                };
            })
            .ToList();

        string? categoryName = null;
        if (product.CategoryId.HasValue)
        {
            categoryName = (await _categoryRepository.GetByIdAsync(product.CategoryId.Value))?.Name;
        }

        var collectionName = (await _collectionRepository.GetByIdAsync(product.CollectionId))?.Name;

        var images = await _productImageRepository.GetByProductIdAsync(product.Id);
        var primaryImagePath = images.FirstOrDefault(i => i.IsPrimary)?.FilePath;
        var imageDtos = images
            .OrderBy(i => i.DisplayOrder)
            .ThenBy(i => i.Id)
            .Select(image => new ProductImageDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageType = image.ImageType,
                FilePath = image.FilePath,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder
            })
            .ToList();

        return new ProductDto
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            CollectionId = product.CollectionId,
            CollectionName = collectionName,
            SurfaceId = product.SurfaceId,
            PrimaryImagePath = primaryImagePath,
            Brand = product.Brand,
            Brands = product.Brands,
            Status = product.Status,
            CommercialName = product.CommercialName,
            ProductGroup = product.ProductGroup,
            Size = product.Size,
            Unit = product.Unit,
            Surface = product.Surface,
            Relief = product.Relief,
            SpecialSurface = product.SpecialSurface,
            FaceCount = product.FaceCount,
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
            DeepAbrasion = NullIfInvalidDeepAbrasion(product.DeepAbrasion),
            HeatResistance = product.HeatResistance,
            AntiSlip = product.AntiSlip,
            GlazedGranite = product.GlazedGranite,
            HasFace = product.HasFace,
            HasVenue = product.HasVenue,
            BoxM2 = product.BoxM2,
            PalletM2 = product.PalletM2,
            DisplayOrder = product.DisplayOrder,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Translations = translationDtos,
            Images = imageDtos
        };
    }

    private static IReadOnlyList<ProductBrand> EffectiveBrands(ProductRequestBase request) =>
        request.Brands.Count > 0 ? request.Brands.Distinct().ToArray() : [request.Brand];

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NullIfBlankOrDash(string? value)
    {
        var normalized = NormalizeOption(value);
        return normalized == "-" ? null : normalized;
    }

    private static string? NullIfInvalidDeepAbrasion(string? value)
    {
        var normalized = NullIfBlankOrDash(value);
        if (normalized is null || normalized.All(char.IsDigit))
        {
            return null;
        }

        return normalized;
    }

    private static bool ContainsMarkup(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains('<');

    private static IReadOnlyList<string> BuildTextOptions(IEnumerable<string?> values) =>
        values
            .Select(NormalizeOption)
            .OfType<string>()
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(value => value)
            .ToList();

    private static IReadOnlyList<string> BuildDeepAbrasionOptions(IEnumerable<string?> values)
    {
        var options = BuildTextOptions(values)
            .Where(value => !value.All(char.IsDigit))
            .ToList();

        if (!options.Contains("≤ 139 mm³", StringComparer.CurrentCultureIgnoreCase))
        {
            options.Insert(0, "≤ 139 mm³");
        }

        return options
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> BuildMultiTextOptions(IEnumerable<string?> values) =>
        BuildTextOptions(values.SelectMany(value => (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)));

    private static IReadOnlyList<decimal> BuildDecimalOptions(IEnumerable<decimal?> values) =>
        values
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Distinct()
            .OrderBy(value => value)
            .ToList();

    private static string? NormalizeOption(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Trim() == "-"
            ? null
            : string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}

