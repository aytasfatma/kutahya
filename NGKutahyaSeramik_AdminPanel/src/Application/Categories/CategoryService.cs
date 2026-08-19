using Application.Products;
using Application.Common;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;

namespace Application.Categories;

public class CategoryService
{
    private const string TrLanguageCode = "TR";

    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetTreeAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        var result = new List<CategoryDto>();
        foreach (var category in categories)
        {
            result.Add(await MapToDtoAsync(category));
        }

        return result;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetExcelSurfaceTypesAsync()
    {
        var categories = await GetTreeAsync();
        var comparer = StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true);
        var surfaceGroups = (await _productRepository.GetAllAsync())
            .Where(product => product.Status == ProductStatus.Active &&
                              product.Brand != ProductBrand.NgPerforma &&
                              !string.IsNullOrWhiteSpace(product.Surface))
            .GroupBy(product => product.Surface!.Trim(), comparer)
            .OrderBy(group => group.Key, comparer)
            .ToList();

        var result = new List<CategoryDto>();
        foreach (var group in surfaceGroups)
        {
            var managed = categories.FirstOrDefault(category =>
                !string.IsNullOrWhiteSpace(category.DisplayName) && comparer.Equals(category.DisplayName.Trim(), group.Key));
            if (managed is null) continue;

            result.Add(new CategoryDto
            {
                Id = managed.Id,
                ParentCategoryId = null,
                ImagePath = managed.ImagePath,
                DisplayOrder = managed.DisplayOrder,
                IsActive = managed.IsActive,
                ProductCount = group.Count(),
                Brands = group.Select(product => product.Brand).Distinct().OrderBy(brand => brand).ToList(),
                Translations = managed.Translations
            });
        }

        return result;
    }

    private async Task SynchronizeExcelSurfaceTypesAsync()
    {
        var comparer = StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true);
        var products = (await _productRepository.GetAllAsync())
            .Where(product => product.Status == ProductStatus.Active &&
                              product.Brand != ProductBrand.NgPerforma &&
                              !string.IsNullOrWhiteSpace(product.Surface))
            .ToList();
        var surfaceGroups = products.GroupBy(product => product.Surface!.Trim(), comparer).ToList();
        var categories = await _categoryRepository.GetAllAsync();
        var categoryDtos = new List<CategoryDto>();
        foreach (var category in categories) categoryDtos.Add(await MapToDtoAsync(category));

        var trLanguage = (await _translationService.GetActiveLanguagesAsync())
            .First(language => string.Equals(language.Code, TrLanguageCode, StringComparison.OrdinalIgnoreCase));

        foreach (var group in surfaceGroups)
        {
            var brands = group.Select(product => product.Brand).Distinct().OrderBy(brand => brand).ToList();
            var existingDto = categoryDtos.FirstOrDefault(category =>
                !string.IsNullOrWhiteSpace(category.DisplayName) && comparer.Equals(category.DisplayName.Trim(), group.Key));
            if (existingDto is not null)
            {
                var existingEntity = categories.First(category => category.Id == existingDto.Id);
                if (!existingEntity.Brands.OrderBy(x => x).SequenceEqual(brands)) existingEntity.SetBrands(brands);
                continue;
            }

            var created = await CreateAsync(new CreateCategoryRequest
            {
                ParentCategoryId = null,
                DisplayOrder = await GetNextDisplayOrderAsync(null),
                Brands = brands,
                Translations = [new CategoryTranslationInput { LanguageId = trLanguage.Id, Name = group.Key }]
            });
            if (!created.Succeeded)
                throw new InvalidOperationException($"Excel yüzeyi oluşturulamadı ({group.Key}): {created.ErrorMessage}");
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public Task<IReadOnlyList<CategoryOptionDto>> GetOptionItemsAsync() =>
        _categoryRepository.GetOptionItemsAsync();

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? null : await MapToDtoAsync(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetParentCandidatesAsync()
    {
        var roots = await _categoryRepository.GetByParentIdAsync(null);

        var result = new List<CategoryDto>();
        foreach (var category in roots)
        {
            result.Add(await MapToDtoAsync(category));
        }

        return result;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<int> GetNextDisplayOrderAsync(int? parentCategoryId)
    {
        var siblings = await _categoryRepository.GetByParentIdAsync(parentCategoryId);
        return siblings.Count == 0 ? 1 : siblings.Max(c => c.DisplayOrder) + 1;
    }

    public async Task<CategoryOperationResult> CreateAsync(CreateCategoryRequest request)
    {
        var validation = await ValidateAsync(
            request.ParentCategoryId, request.DisplayOrder, request.Translations, excludeCategoryId: null);

        if (!validation.Succeeded)
        {
            return validation;
        }

        var category = new Category(request.ParentCategoryId, request.ImagePath, request.DisplayOrder);
        var trInput = await GetTrInputAsync(request.Translations);
        category.SetIdentity(trInput!.Name!, trInput.SeoUrl);
        category.SetBrands(request.Brands);
        await _categoryRepository.AddAsync(category);

        // Yeni Category'nin Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik,
        // gerçek FK/navigation değil — ADR-012), bu yüzden Create'te iki SaveChanges çağrısı gerekir.
        await _unitOfWork.SaveChangesAsync();

        return CategoryOperationResult.Success();
    }

    public async Task<CategoryOperationResult> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return CategoryOperationResult.Failure("Kategori bulunamadı.");
        }

        if (request.ParentCategoryId == id)
        {
            return CategoryOperationResult.Failure("Bir kategori kendi üst kategorisi olamaz.");
        }

        if (request.ParentCategoryId.HasValue)
        {
            var hasChildren = await _categoryRepository.HasChildrenAsync(id);
            if (hasChildren)
            {
                return CategoryOperationResult.Failure(
                    "Alt kategorisi olan bir kategori başka bir kategorinin altına taşınamaz.");
            }
        }

        var validation = await ValidateAsync(
            request.ParentCategoryId, request.DisplayOrder, request.Translations, excludeCategoryId: id);

        if (!validation.Succeeded)
        {
            return validation;
        }

        category.UpdateDetails(request.ParentCategoryId, request.ImagePath, request.DisplayOrder);
        var trInput = await GetTrInputAsync(request.Translations);
        category.SetIdentity(trInput!.Name!, trInput.SeoUrl);
        category.SetBrands(request.Brands);

        await _unitOfWork.SaveChangesAsync();

        return CategoryOperationResult.Success();
    }

    public async Task<CategoryOperationResult> ToggleActiveAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return CategoryOperationResult.Failure("Kategori bulunamadı.");
        }

        if (category.IsActive)
        {
            category.Deactivate();
        }
        else
        {
            category.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return CategoryOperationResult.Success();
    }

    public async Task<CategoryOperationResult> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return CategoryOperationResult.Failure("Kategori bulunamadı.");
        }

        var hasChildren = await _categoryRepository.HasChildrenAsync(id);
        if (hasChildren)
        {
            return CategoryOperationResult.Failure("Alt kategorisi olan bir kategori silinemez.");
        }

        // Task 5'te Product.CategoryId FK'si Restrict olarak eklendi — referanslı bir kategori
        // silinmeye çalışılırsa DB seviyesinde hata fırlar; burada önceden kontrol edilip
        // kullanıcıya anlaşılır bir mesajla engellenir.
        var hasProducts = await _productRepository.HasAnyWithCategoryIdAsync(id);
        if (hasProducts)
        {
            return CategoryOperationResult.Failure("Bu kategoriye bağlı ürünler olduğu için kategori silinemez.");
        }

        _categoryRepository.Remove(category);

        await _unitOfWork.SaveChangesAsync();
        return CategoryOperationResult.Success();
    }

    private async Task<CategoryOperationResult> ValidateAsync(
        int? parentCategoryId,
        int displayOrder,
        IReadOnlyList<CategoryTranslationInput> translations,
        int? excludeCategoryId)
    {
        if (displayOrder < 1)
        {
            return CategoryOperationResult.Failure(SortOrderValidationMessages.Minimum);
        }

        if (parentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(parentCategoryId.Value);
            if (parent is null)
            {
                return CategoryOperationResult.Failure("Seçilen üst kategori bulunamadı.");
            }

            if (parent.ParentCategoryId.HasValue)
            {
                return CategoryOperationResult.Failure(
                    "Kategori ağacı en fazla iki seviyeli olabilir; bir alt kategori üst kategori olarak seçilemez.");
            }
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return CategoryOperationResult.Failure("Aktif Türkçe (TR) dili bulunamadı.");
        }

        var trInput = translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Name))
        {
            return CategoryOperationResult.Failure("Türkçe (TR) kategori adı zorunludur.");
        }

        var isDuplicate = await IsDuplicateNameAsync(parentCategoryId, trInput.Name.Trim(), excludeCategoryId);
        if (isDuplicate)
        {
            return CategoryOperationResult.Failure(
                "Aynı üst kategori altında bu isimde başka bir kategori zaten var.");
        }

        var siblings = await _categoryRepository.GetByParentIdAsync(parentCategoryId);
        if (SortOrderValidation.HasDuplicate(siblings, displayOrder, excludeCategoryId, c => c.Id, c => c.DisplayOrder))
        {
            return CategoryOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        return CategoryOperationResult.Success();
    }

    private async Task<bool> IsDuplicateNameAsync(int? parentCategoryId, string trName, int? excludeCategoryId)
    {
        var siblings = await _categoryRepository.GetByParentIdAsync(parentCategoryId);

        foreach (var sibling in siblings)
        {
            if (excludeCategoryId.HasValue && sibling.Id == excludeCategoryId.Value)
            {
                continue;
            }

            var siblingName = sibling.Name;

            if (siblingName is not null && string.Equals(siblingName.Trim(), trName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async Task SaveTranslationsAsync(int categoryId, IReadOnlyList<CategoryTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(categoryId, input.LanguageId, CategoryFields.Name, input.Name);
            await SaveFieldAsync(categoryId, input.LanguageId, CategoryFields.Description, input.Description);
            await SaveFieldAsync(categoryId, input.LanguageId, CategoryFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(categoryId, input.LanguageId, CategoryFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(categoryId, input.LanguageId, CategoryFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int categoryId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.Category, categoryId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.Category, categoryId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<CategoryDto> MapToDtoAsync(Category category)
    {
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                return new CategoryTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Name = language.Code == TrLanguageCode ? category.Name : null,
                    SeoUrl = language.Code == TrLanguageCode ? category.SeoUrl : null
                };
            })
            .ToList();

        var linkedProducts = (await _productRepository.GetAllAsync())
            .Where(product => product.CategoryId == category.Id && product.Status == ProductStatus.Active)
            .ToList();
        var productCount = linkedProducts.Count;
        var linkedBrands = linkedProducts
            .SelectMany(product => product.Brands)
            .Distinct()
            .OrderBy(brand => brand)
            .ToList();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            SeoUrl = category.SeoUrl,
            ParentCategoryId = category.ParentCategoryId,
            ImagePath = category.ImagePath,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = productCount,
            Brands = linkedBrands,
            Translations = translationDtos
        };
    }

    private async Task<CategoryTranslationInput?> GetTrInputAsync(IReadOnlyList<CategoryTranslationInput> inputs)
    {
        var tr = (await _translationService.GetActiveLanguagesAsync()).FirstOrDefault(x => x.Code == TrLanguageCode);
        return tr is null ? null : inputs.FirstOrDefault(x => x.LanguageId == tr.Id);
    }
}

