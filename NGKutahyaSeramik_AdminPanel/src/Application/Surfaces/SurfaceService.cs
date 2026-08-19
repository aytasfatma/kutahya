using Application.Common;
using Domain.Entities;
using Application.Products;
using Application.Collections;
using Application.Translations;
using Domain.Enums;

namespace Application.Surfaces;

public class SurfaceService
{
    private readonly ISurfaceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly ITranslationService _translationService;

    public SurfaceService(ISurfaceRepository repository, IUnitOfWork unitOfWork, IProductRepository productRepository, ITranslationService translationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _translationService = translationService;
    }

    public Task<IReadOnlyList<Surface>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Surface?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() => _translationService.GetActiveLanguagesAsync();
    public async Task<IReadOnlyList<TranslationValue>> GetTranslationsAsync(int id)
    {
        var surface = await _repository.GetByIdAsync(id);
        var tr = (await _translationService.GetActiveLanguagesAsync()).FirstOrDefault(x => x.Code == "TR");
        if (surface is null || tr is null) return [];
        var values = new List<TranslationValue> { new(tr.Id, tr.Code, CollectionFields.Name, surface.Name) };
        if (!string.IsNullOrWhiteSpace(surface.SeoUrl)) values.Add(new(tr.Id, tr.Code, CollectionFields.SeoUrl, surface.SeoUrl));
        return values;
    }

    public async Task<IReadOnlyList<SurfaceListItemDto>> GetListAsync()
    {
        var surfaces = await _repository.GetAllAsync();
        var products = await _productRepository.GetAllAsync();
        return surfaces.Select(surface =>
        {
            var linked = products.Where(product => product.SurfaceId == surface.Id).ToList();
            return new SurfaceListItemDto
            {
                Id = surface.Id,
                Name = surface.Name,
                ProductCount = linked.Count,
                Brands = linked.SelectMany(product => product.Brands).Distinct().OrderBy(brand => brand).ToList(),
                IsActive = surface.IsActive
            };
        }).ToList();
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(string name, int displayOrder)
        => await CreateAsync(name, null, Enum.GetValues<ProductBrand>(), displayOrder, []);

    public async Task<(bool Succeeded, string? Error)> CreateAsync(string name, string? imagePath, IReadOnlyList<ProductBrand> brands, int displayOrder, IReadOnlyList<CollectionTranslationInput> translations)
    {
        if (string.IsNullOrWhiteSpace(name)) return (false, "Yüzey adı zorunludur.");
        if (displayOrder < 1) return (false, "Sıralama 1 veya daha büyük olmalıdır.");
        if (await _repository.IsNameInUseAsync(name.Trim())) return (false, "Bu yüzey zaten mevcut.");
        if ((await _repository.GetAllAsync()).Any(x => x.DisplayOrder == displayOrder)) return (false, "Bu sıralama değeri kullanılıyor.");
        if (brands.Count == 0) return (false, "En az bir marka seçmelisiniz.");
        var surface = new Surface(name, displayOrder);
        surface.SetContent(imagePath, brands);
        surface.SetSeoUrl(translations.FirstOrDefault()?.SeoUrl);
        await _repository.AddAsync(surface);
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(int id, string name, int displayOrder)
        => await UpdateAsync(id, name, null, Enum.GetValues<ProductBrand>(), displayOrder, []);

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(int id, string name, string? imagePath, IReadOnlyList<ProductBrand> brands, int displayOrder, IReadOnlyList<CollectionTranslationInput> translations)
    {
        var surface = await _repository.GetByIdAsync(id);
        if (surface is null) return (false, "Yüzey bulunamadı.");
        if (string.IsNullOrWhiteSpace(name)) return (false, "Yüzey adı zorunludur.");
        if (await _repository.IsNameInUseAsync(name.Trim(), id)) return (false, "Bu yüzey zaten mevcut.");
        if ((await _repository.GetAllAsync()).Any(x => x.Id != id && x.DisplayOrder == displayOrder)) return (false, "Bu sıralama değeri kullanılıyor.");
        surface.Update(name, displayOrder);
        surface.SetContent(imagePath, brands);
        surface.SetSeoUrl(translations.FirstOrDefault()?.SeoUrl);
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> ToggleAsync(int id)
    {
        var surface = await _repository.GetByIdAsync(id);
        if (surface is null) return (false, "Yüzey bulunamadı.");
        if (surface.IsActive) surface.Deactivate(); else surface.Activate();
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteAsync(int id)
    {
        var surface = await _repository.GetByIdAsync(id);
        if (surface is null) return (false, "Yüzey bulunamadı.");
        if (await _repository.HasProductsAsync(id)) return (false, "Bu yüzeye bağlı ürünler olduğu için silinemez.");
        _repository.Remove(surface);
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    private async Task SaveTranslationsAsync(int id, IReadOnlyList<CollectionTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(id, input.LanguageId, CollectionFields.Name, input.Name);
            await SaveFieldAsync(id, input.LanguageId, CollectionFields.Description, input.Description);
            await SaveFieldAsync(id, input.LanguageId, CollectionFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(id, input.LanguageId, CollectionFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(id, input.LanguageId, CollectionFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int id, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) await _translationService.DeleteTranslationFieldAsync(EntityType.Surface, id, languageId, fieldName);
        else await _translationService.SetTranslationAsync(EntityType.Surface, id, languageId, fieldName, value.Trim());
    }
}
