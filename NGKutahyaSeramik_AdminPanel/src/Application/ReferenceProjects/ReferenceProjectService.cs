using Application.Products;
using Application.Common;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;

namespace Application.ReferenceProjects;

public class ReferenceProjectService
{
    private const string TrLanguageCode = "TR";

    private readonly IReferenceProjectRepository _referenceProjectRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReferenceProjectImageRepository _referenceProjectImageRepository;
    private readonly ReferenceProjectImageService _referenceProjectImageService;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public ReferenceProjectService(
        IReferenceProjectRepository referenceProjectRepository,
        IProductRepository productRepository,
        IReferenceProjectImageRepository referenceProjectImageRepository,
        ReferenceProjectImageService referenceProjectImageService,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _referenceProjectRepository = referenceProjectRepository;
        _productRepository = productRepository;
        _referenceProjectImageRepository = referenceProjectImageRepository;
        _referenceProjectImageService = referenceProjectImageService;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<(int Id, string Label)>> GetProductOptionsAsync()
    {
        return await _productRepository.GetProductCodeOptionsAsync();
    }

    public async Task<IReadOnlyList<ReferenceProjectDto>> GetAllAsync()
    {
        var referenceProjects = await _referenceProjectRepository.GetAllAsync();

        var result = new List<ReferenceProjectDto>();
        foreach (var referenceProject in referenceProjects)
        {
            result.Add(await MapToDtoAsync(referenceProject));
        }

        return result;
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var referenceProjects = await _referenceProjectRepository.GetAllAsync();
        return SortOrderValidation.Next(referenceProjects, rp => rp.DisplayOrder);
    }

    public async Task<ReferenceProjectDto?> GetByIdAsync(int id)
    {
        var referenceProject = await _referenceProjectRepository.GetByIdAsync(id);
        return referenceProject is null ? null : await MapToDtoAsync(referenceProject);
    }

    public async Task<ReferenceProjectOperationResult> CreateAsync(CreateReferenceProjectRequest request)
    {
        var validation = await ValidateAsync(request, excludeReferenceProjectId: null);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var referenceProject = new ReferenceProject(
            NullIfBlank(request.Location),
            request.Region,
            request.Brand,
            request.ProjectType,
            NullIfBlank(request.Architect),
            request.Year,
            request.DisplayOrder);

        await _referenceProjectRepository.AddAsync(referenceProject);

        // Yeni ReferenceProject'in Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId
        // polimorfik, gerçek FK/navigation değil — ADR-012), bu yüzden Create'te iki SaveChanges
        // çağrısı gerekir (Category/Collection/Product ile aynı desen).
        await _unitOfWork.SaveChangesAsync();

        await SaveTranslationsAsync(referenceProject.Id, request.Translations);
        await _referenceProjectRepository.ReplaceProductRelationsAsync(
            referenceProject.Id, request.RelatedProductIds.Distinct().ToList());
        await _unitOfWork.SaveChangesAsync();

        return ReferenceProjectOperationResult.Success();
    }

    public async Task<ReferenceProjectOperationResult> UpdateAsync(int id, UpdateReferenceProjectRequest request)
    {
        var referenceProject = await _referenceProjectRepository.GetByIdAsync(id);
        if (referenceProject is null)
        {
            return ReferenceProjectOperationResult.Failure("Referans proje bulunamadı.");
        }

        var validation = await ValidateAsync(request, excludeReferenceProjectId: id);
        if (!validation.Succeeded)
        {
            return validation;
        }

        referenceProject.UpdateDetails(
            NullIfBlank(request.Location),
            request.Region,
            request.Brand,
            request.ProjectType,
            NullIfBlank(request.Architect),
            request.Year,
            request.DisplayOrder);

        await SaveTranslationsAsync(referenceProject.Id, request.Translations);
        await _referenceProjectRepository.ReplaceProductRelationsAsync(
            referenceProject.Id, request.RelatedProductIds.Distinct().ToList());
        await _unitOfWork.SaveChangesAsync();

        return ReferenceProjectOperationResult.Success();
    }

    public async Task<ReferenceProjectOperationResult> ToggleActiveAsync(int id)
    {
        var referenceProject = await _referenceProjectRepository.GetByIdAsync(id);
        if (referenceProject is null)
        {
            return ReferenceProjectOperationResult.Failure("Referans proje bulunamadı.");
        }

        if (referenceProject.IsActive)
        {
            referenceProject.Deactivate();
        }
        else
        {
            referenceProject.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return ReferenceProjectOperationResult.Success();
    }

    public async Task<ReferenceProjectOperationResult> DeleteAsync(int id)
    {
        var referenceProject = await _referenceProjectRepository.GetByIdAsync(id);
        if (referenceProject is null)
        {
            return ReferenceProjectOperationResult.Failure("Referans proje bulunamadı.");
        }

        // Ürün ilişki satırları DB'de Cascade FK ile otomatik silinir (bkz. ProductReferenceProjectConfiguration).
        // Görsel temizliği (DB kaydı + fiziksel dosya) proje silinmeden önce ReferenceProjectImageService
        // üzerinden yapılır — DB cascade FK'si zaten kayıtları silecektir, ama fiziksel dosya
        // temizliğini garanti etmek için açık çağrı gerekir (ProductImage/Task 5.1 ile aynı desen).
        await _referenceProjectImageService.DeleteAllForReferenceProjectAsync(id);
        await _translationService.DeleteTranslationsForAsync(EntityType.ReferenceProject, id);
        _referenceProjectRepository.Remove(referenceProject);

        await _unitOfWork.SaveChangesAsync();
        return ReferenceProjectOperationResult.Success();
    }

    private async Task<ReferenceProjectOperationResult> ValidateAsync(ReferenceProjectRequestBase request, int? excludeReferenceProjectId)
    {
        if (request.Year is < 1900 or > 2030)
        {
            return ReferenceProjectOperationResult.Failure("Yıl 1900 ile 2030 arasında olmalıdır.");
        }

        if (request.DisplayOrder < 1)
        {
            return ReferenceProjectOperationResult.Failure(SortOrderValidationMessages.Minimum);
        }

        var referenceProjects = await _referenceProjectRepository.GetAllAsync();
        if (SortOrderValidation.HasDuplicate(referenceProjects, request.DisplayOrder, excludeReferenceProjectId, rp => rp.Id, rp => rp.DisplayOrder))
        {
            return ReferenceProjectOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        foreach (var productId in request.RelatedProductIds.Distinct())
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is null)
            {
                return ReferenceProjectOperationResult.Failure($"Seçilen ürün (Id={productId}) bulunamadı.");
            }
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return ReferenceProjectOperationResult.Failure("Aktif Türkçe (TR) dili bulunamadı.");
        }

        var trInput = request.Translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.ProjectName))
        {
            return ReferenceProjectOperationResult.Failure("Türkçe (TR) proje adı zorunludur.");
        }

        return ReferenceProjectOperationResult.Success();
    }

    private async Task SaveTranslationsAsync(int referenceProjectId, IReadOnlyList<ReferenceProjectTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(referenceProjectId, input.LanguageId, ReferenceProjectFields.ProjectName, input.ProjectName);
            await SaveFieldAsync(referenceProjectId, input.LanguageId, ReferenceProjectFields.Description, input.Description);
            await SaveFieldAsync(referenceProjectId, input.LanguageId, ReferenceProjectFields.SeoUrl, input.SeoUrl);
        }
    }

    private async Task SaveFieldAsync(int referenceProjectId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.ReferenceProject, referenceProjectId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.ReferenceProject, referenceProjectId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<ReferenceProjectDto> MapToDtoAsync(ReferenceProject referenceProject)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.ReferenceProject, referenceProject.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new ReferenceProjectTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    ProjectName = Get(ReferenceProjectFields.ProjectName),
                    Description = Get(ReferenceProjectFields.Description),
                    SeoUrl = Get(ReferenceProjectFields.SeoUrl)
                };
            })
            .ToList();

        var relatedProductIds = await _referenceProjectRepository.GetRelatedProductIdsAsync(referenceProject.Id);
        var relatedProducts = new List<ReferenceProjectRelatedProductDto>();
        foreach (var productId in relatedProductIds)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is not null)
            {
                relatedProducts.Add(new ReferenceProjectRelatedProductDto
                {
                    Id = product.Id,
                    Label = product.ProductCode,
                    Name = product.CommercialName ?? product.ProductCode,
                    SeoUrl = product.ProductCode.ToLowerInvariant(),
                    Brand = product.Brand.ToString()
                });
            }
        }

        var images = await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(referenceProject.Id);
        var featuredImagePath = images.FirstOrDefault(i => i.IsFeatured)?.FilePath;

        return new ReferenceProjectDto
        {
            Id = referenceProject.Id,
            Location = referenceProject.Location,
            Region = referenceProject.Region,
            Brand = referenceProject.Brand,
            ProjectType = referenceProject.ProjectType,
            Architect = referenceProject.Architect,
            Year = referenceProject.Year,
            DisplayOrder = referenceProject.DisplayOrder,
            IsActive = referenceProject.IsActive,
            FeaturedImagePath = featuredImagePath,
            Translations = translationDtos,
            RelatedProducts = relatedProducts
        };
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

