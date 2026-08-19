using Application.Products;
using Application.Common;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;

namespace Application.Collections;

public class CollectionService
{
    private const string TrLanguageCode = "TR";

    private readonly ICollectionRepository _collectionRepository;
    private readonly IProductRepository _productRepository;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public CollectionService(
        ICollectionRepository collectionRepository,
        IProductRepository productRepository,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _collectionRepository = collectionRepository;
        _productRepository = productRepository;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CollectionDto>> GetAllAsync()
    {
        var collections = await _collectionRepository.GetAllAsync();
        var products = await _productRepository.GetAllAsync();
        var productCountsByCollectionId = products
            .GroupBy(p => p.CollectionId)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<CollectionDto>();
        foreach (var collection in collections)
        {
            var linkedProducts = products.Where(p => p.CollectionId == collection.Id && p.Status == ProductStatus.Active).ToList();
            result.Add(await MapToDtoAsync(
                collection,
                productCountsByCollectionId.GetValueOrDefault(collection.Id),
                linkedProducts.SelectMany(p => p.Brands).Distinct().OrderBy(x => x).ToList()));
        }

        return result;
    }

    public Task<IReadOnlyList<CollectionOptionDto>> GetOptionItemsAsync() =>
        _collectionRepository.GetOptionItemsAsync();

    public async Task<CollectionDto?> GetByIdAsync(int id)
    {
        var collection = await _collectionRepository.GetByIdAsync(id);
        if (collection is null)
        {
            return null;
        }

        var products = await _productRepository.GetAllAsync();
        var productCount = products.Count(p => p.CollectionId == collection.Id);
        var brands = products.Where(p => p.CollectionId == collection.Id && p.Status == ProductStatus.Active)
            .SelectMany(p => p.Brands).Distinct().OrderBy(x => x).ToList();

        return await MapToDtoAsync(collection, productCount, brands);
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var collections = await _collectionRepository.GetAllAsync();
        return SortOrderValidation.Next(collections, c => c.DisplayOrder);
    }

    public async Task<CollectionOperationResult> CreateAsync(CreateCollectionRequest request)
    {
        var validation = await ValidateAsync(request.DisplayOrder, request.Translations, excludeCollectionId: null);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var collection = new Collection(request.ImagePath, request.DisplayOrder);
        var trInput = await GetTrInputAsync(request.Translations);
        collection.SetIdentity(trInput!.Name!, trInput.SeoUrl);
        collection.SetBrands(request.Brands);
        await _collectionRepository.AddAsync(collection);

        // Yeni Collection'ın Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik,
        // gerçek FK/navigation değil — ADR-012), bu yüzden Create'te iki SaveChanges çağrısı gerekir.
        await _unitOfWork.SaveChangesAsync();

        return CollectionOperationResult.Success();
    }

    public async Task<CollectionOperationResult> UpdateAsync(int id, UpdateCollectionRequest request)
    {
        var collection = await _collectionRepository.GetByIdAsync(id);
        if (collection is null)
        {
            return CollectionOperationResult.Failure("Koleksiyon bulunamadı.");
        }

        var validation = await ValidateAsync(request.DisplayOrder, request.Translations, excludeCollectionId: id);
        if (!validation.Succeeded)
        {
            return validation;
        }

        collection.UpdateDetails(request.ImagePath, request.DisplayOrder);
        var trInput = await GetTrInputAsync(request.Translations);
        collection.SetIdentity(trInput!.Name!, trInput.SeoUrl);
        collection.SetBrands(request.Brands);

        await _unitOfWork.SaveChangesAsync();

        return CollectionOperationResult.Success();
    }

    public async Task<CollectionOperationResult> ToggleActiveAsync(int id)
    {
        var collection = await _collectionRepository.GetByIdAsync(id);
        if (collection is null)
        {
            return CollectionOperationResult.Failure("Koleksiyon bulunamadı.");
        }

        if (collection.IsActive)
        {
            collection.Deactivate();
        }
        else
        {
            collection.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return CollectionOperationResult.Success();
    }

    public async Task<CollectionOperationResult> DeleteAsync(int id)
    {
        var collection = await _collectionRepository.GetByIdAsync(id);
        if (collection is null)
        {
            return CollectionOperationResult.Failure("Koleksiyon bulunamadı.");
        }

        // Task 5'te Product.CollectionId FK'si Restrict olarak eklendi — referanslı bir koleksiyon
        // silinmeye çalışılırsa DB seviyesinde hata fırlar; burada önceden kontrol edilip
        // kullanıcıya anlaşılır bir mesajla engellenir. Document ilişkisi Cascade olduğu için
        // (Task 5.1B/#9) buna ihtiyaç yok — koleksiyon silindiğinde ilişkili Document kaydı etkilenmez.
        var hasProducts = await _productRepository.HasAnyWithCollectionIdAsync(id);
        if (hasProducts)
        {
            return CollectionOperationResult.Failure("Bu koleksiyona bağlı ürünler olduğu için koleksiyon silinemez.");
        }

        _collectionRepository.Remove(collection);

        await _unitOfWork.SaveChangesAsync();
        return CollectionOperationResult.Success();
    }

    private async Task<CollectionOperationResult> ValidateAsync(
        int displayOrder,
        IReadOnlyList<CollectionTranslationInput> translations,
        int? excludeCollectionId)
    {
        if (displayOrder < 1)
        {
            return CollectionOperationResult.Failure(SortOrderValidationMessages.Minimum);
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return CollectionOperationResult.Failure("Aktif Türkçe (TR) dili bulunamadı.");
        }

        var trInput = translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Name))
        {
            return CollectionOperationResult.Failure("Türkçe (TR) koleksiyon adı zorunludur.");
        }

        var isDuplicate = await IsDuplicateNameAsync(trInput.Name.Trim(), excludeCollectionId);
        if (isDuplicate)
        {
            return CollectionOperationResult.Failure("Bu isimde başka bir koleksiyon zaten var.");
        }

        var allCollections = await _collectionRepository.GetAllAsync();
        if (SortOrderValidation.HasDuplicate(allCollections, displayOrder, excludeCollectionId, c => c.Id, c => c.DisplayOrder))
        {
            return CollectionOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        return CollectionOperationResult.Success();
    }

    private async Task<bool> IsDuplicateNameAsync(string trName, int? excludeCollectionId)
    {
        var allCollections = await _collectionRepository.GetAllAsync();

        foreach (var collection in allCollections)
        {
            if (excludeCollectionId.HasValue && collection.Id == excludeCollectionId.Value)
            {
                continue;
            }

            var name = collection.Name;

            if (name is not null && string.Equals(name.Trim(), trName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async Task SaveTranslationsAsync(int collectionId, IReadOnlyList<CollectionTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(collectionId, input.LanguageId, CollectionFields.Name, input.Name);
            await SaveFieldAsync(collectionId, input.LanguageId, CollectionFields.Description, input.Description);
            await SaveFieldAsync(collectionId, input.LanguageId, CollectionFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(collectionId, input.LanguageId, CollectionFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(collectionId, input.LanguageId, CollectionFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int collectionId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.Collection, collectionId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.Collection, collectionId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<CollectionDto> MapToDtoAsync(Collection collection, int productCount, IReadOnlyList<ProductBrand> brands)
    {
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                return new CollectionTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Name = language.Code == TrLanguageCode ? collection.Name : null,
                    SeoUrl = language.Code == TrLanguageCode ? collection.SeoUrl : null
                };
            })
            .ToList();

        return new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            SeoUrl = collection.SeoUrl,
            ImagePath = collection.ImagePath,
            DisplayOrder = collection.DisplayOrder,
            IsActive = collection.IsActive,
            ProductCount = productCount,
            Brands = brands,
            Translations = translationDtos
        };
    }

    private async Task<CollectionTranslationInput?> GetTrInputAsync(IReadOnlyList<CollectionTranslationInput> inputs)
    {
        var tr = (await _translationService.GetActiveLanguagesAsync()).FirstOrDefault(x => x.Code == TrLanguageCode);
        return tr is null ? null : inputs.FirstOrDefault(x => x.LanguageId == tr.Id);
    }
}

