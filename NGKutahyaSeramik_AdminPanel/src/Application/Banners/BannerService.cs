using Application.Storage;
using Application.Common;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Banners;

/// <summary>
/// Madde 16.1 — Hero Banner. Görsel yükleme/değiştirme/silme BlogService/NewsService ile birebir aynı
/// desen (tekil alan, ProductImage'ın doğrulama/güvenlik mantığı). Status enum yerine bool IsActive +
/// ToggleActive kullanılıyor (Category/Collection deseni) — doküman burada yalnızca 2 durum istiyor.
/// </summary>
public class BannerService
{
    private const string TrLanguageCode = "TR";
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedExtensionContentTypes =
        new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly IBannerRepository _bannerRepository;
    private readonly ITranslationService _translationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BannerService> _logger;

    public BannerService(
        IBannerRepository bannerRepository,
        ITranslationService translationService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<BannerService> logger)
    {
        _bannerRepository = bannerRepository;
        _translationService = translationService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<BannerDto>> GetAllAsync()
    {
        var banners = await _bannerRepository.GetAllAsync();

        var result = new List<BannerDto>();
        foreach (var banner in banners)
        {
            result.Add(await MapToDtoAsync(banner));
        }

        return result;
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var banners = await _bannerRepository.GetAllAsync();
        return SortOrderValidation.Next(banners, b => b.DisplayOrder);
    }

    public async Task<BannerDto?> GetByIdAsync(int id)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        return banner is null ? null : await MapToDtoAsync(banner);
    }

    public async Task<BannerOperationResult> CreateAsync(CreateBannerRequest request)
    {
        var metadataValidation = await ValidateMetadataAsync(request, excludeBannerId: null);
        if (metadataValidation is not null)
        {
            return BannerOperationResult.Failure(metadataValidation);
        }

        var hasImage = request.ImageLength is > 0 && request.ImageContent is not null;
        (string? Error, string? Extension) imageValidation = (null, null);
        if (hasImage)
        {
            imageValidation = await ValidateImageAsync(
                request.ImageOriginalFileName!, request.ImageContentType!,
                request.ImageLength!.Value, request.ImageContent!);
            if (imageValidation.Error is not null)
            {
                return BannerOperationResult.Failure(imageValidation.Error);
            }
        }

        var banner = new Banner(request.PublishStartDate, request.PublishEndDate, request.DisplayOrder);
        await _bannerRepository.AddAsync(banner);

        // Yeni Banner'ın Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik +
        // görsel klasör anahtarı Id bazlı), bu yüzden Create'te iki SaveChanges çağrısı gerekir.
        await _unitOfWork.SaveChangesAsync();

        if (hasImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"banners/{banner.Id}";

            string savedPath;
            try
            {
                savedPath = await _fileStorageService.SaveAsync(folder, request.ImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Banner görseli diske kaydedilirken hata oluştu. BannerId={BannerId}", banner.Id);
                return BannerOperationResult.Failure("Görsel kaydedilirken bir hata oluştu.");
            }

            banner.SetImagePath(savedPath);
        }

        await SaveTranslationsAsync(banner.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return BannerOperationResult.Success();
    }

    public async Task<BannerOperationResult> UpdateAsync(int id, UpdateBannerRequest request)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner is null)
        {
            return BannerOperationResult.Failure("Banner bulunamadı.");
        }

        var metadataValidation = await ValidateMetadataAsync(request, excludeBannerId: id);
        if (metadataValidation is not null)
        {
            return BannerOperationResult.Failure(metadataValidation);
        }

        var replacingImage = request.ImageLength is > 0 && request.ImageContent is not null;
        (string? Error, string? Extension) imageValidation = (null, null);
        if (replacingImage)
        {
            imageValidation = await ValidateImageAsync(
                request.ImageOriginalFileName!, request.ImageContentType!,
                request.ImageLength!.Value, request.ImageContent!);
            if (imageValidation.Error is not null)
            {
                return BannerOperationResult.Failure(imageValidation.Error);
            }
        }

        var oldImagePath = banner.ImagePath;
        string? newSavedPath = null;

        if (replacingImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"banners/{banner.Id}";

            try
            {
                newSavedPath = await _fileStorageService.SaveAsync(folder, request.ImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Banner görseli değiştirilirken diske kaydetme hatası oluştu. BannerId={BannerId}", id);
                return BannerOperationResult.Failure("Görsel kaydedilirken bir hata oluştu.");
            }
        }

        banner.UpdateDetails(request.PublishStartDate, request.PublishEndDate, request.DisplayOrder);

        if (replacingImage)
        {
            banner.SetImagePath(newSavedPath);
        }
        else if (request.RemoveImage)
        {
            banner.SetImagePath(null);
        }

        await SaveTranslationsAsync(banner.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        if ((replacingImage || request.RemoveImage) && oldImagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(oldImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski banner görseli silinemedi. FilePath={FilePath}", oldImagePath);
            }
        }

        return BannerOperationResult.Success();
    }

    public async Task<BannerOperationResult> ToggleActiveAsync(int id)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner is null)
        {
            return BannerOperationResult.Failure("Banner bulunamadı.");
        }

        if (banner.IsActive)
        {
            banner.Deactivate();
        }
        else
        {
            banner.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return BannerOperationResult.Success();
    }

    public async Task<BannerOperationResult> DeleteAsync(int id)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner is null)
        {
            return BannerOperationResult.Failure("Banner bulunamadı.");
        }

        var imagePath = banner.ImagePath;

        await _translationService.DeleteTranslationsForAsync(EntityType.Banner, id);
        _bannerRepository.Remove(banner);

        await _unitOfWork.SaveChangesAsync();

        if (imagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(imagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Banner veritabanından silindi ama görsel silinemedi. FilePath={FilePath}", imagePath);
            }
        }

        return BannerOperationResult.Success();
    }

    private async Task<string?> ValidateMetadataAsync(BannerRequestBase request, int? excludeBannerId)
    {
        if (request.DisplayOrder < 1)
        {
            return SortOrderValidationMessages.Minimum;
        }

        var banners = await _bannerRepository.GetAllAsync();
        if (SortOrderValidation.HasDuplicate(banners, request.DisplayOrder, excludeBannerId, b => b.Id, b => b.DisplayOrder))
        {
            return SortOrderValidationMessages.Duplicate;
        }

        if (request.PublishStartDate.HasValue && request.PublishEndDate.HasValue &&
            request.PublishStartDate.Value > request.PublishEndDate.Value)
        {
            return "Yayın başlangıç tarihi bitiş tarihinden sonra olamaz.";
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return "Aktif Türkçe (TR) dili bulunamadı.";
        }

        var trInput = request.Translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Title))
        {
            return "Türkçe (TR) banner başlığı zorunludur.";
        }

        return null;
    }

    private async Task<(string? Error, string? Extension)> ValidateImageAsync(
        string originalFileName, string contentType, long length, Stream content)
    {
        if (length <= 0)
        {
            return ("Boş dosya yüklenemez.", null);
        }

        if (length > MaxImageSizeBytes)
        {
            return ($"Dosya boyutu {MaxImageSizeBytes / (1024 * 1024)} MB sınırını aşıyor.", null);
        }

        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensionContentTypes.TryGetValue(extension, out var expectedContentType))
        {
            return ("İzin verilmeyen dosya uzantısı. Yalnızca .jpg, .jpeg, .png, .webp kabul edilir.", null);
        }

        if (!string.Equals(contentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return ("Dosya uzantısı ile içerik tipi (MIME) uyuşmuyor.", null);
        }

        if (!content.CanSeek)
        {
            return ("Dosya akışı okunamadı.", null);
        }

        var header = new byte[12];
        content.Position = 0;
        var bytesRead = await content.ReadAsync(header.AsMemory(0, header.Length));
        content.Position = 0;

        if (!HasValidImageSignature(header, bytesRead, extension))
        {
            return ("Dosya içeriği belirtilen görsel formatıyla uyuşmuyor.", null);
        }

        return (null, extension);
    }

    private static bool HasValidImageSignature(byte[] header, int bytesRead, string extension)
    {
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            case ".png":
                return bytesRead >= 8 &&
                    header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            case ".webp":
                return bytesRead >= 12 &&
                    header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
                    header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P';
            default:
                return false;
        }
    }

    private async Task SaveTranslationsAsync(int bannerId, IReadOnlyList<BannerTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(bannerId, input.LanguageId, BannerFields.Title, input.Title);
            await SaveFieldAsync(bannerId, input.LanguageId, BannerFields.Subtitle, input.Subtitle);
            await SaveFieldAsync(bannerId, input.LanguageId, BannerFields.ButtonText, input.ButtonText);
            await SaveFieldAsync(bannerId, input.LanguageId, BannerFields.ButtonUrl, input.ButtonUrl);
        }
    }

    private async Task SaveFieldAsync(int bannerId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.Banner, bannerId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.Banner, bannerId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<BannerDto> MapToDtoAsync(Banner banner)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.Banner, banner.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new BannerTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Title = Get(BannerFields.Title),
                    Subtitle = Get(BannerFields.Subtitle),
                    ButtonText = Get(BannerFields.ButtonText),
                    ButtonUrl = Get(BannerFields.ButtonUrl)
                };
            })
            .ToList();

        return new BannerDto
        {
            Id = banner.Id,
            ImagePath = banner.ImagePath,
            PublishStartDate = banner.PublishStartDate,
            PublishEndDate = banner.PublishEndDate,
            DisplayOrder = banner.DisplayOrder,
            IsActive = banner.IsActive,
            Translations = translationDtos
        };
    }
}

