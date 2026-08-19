using Application.Storage;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Pages;

/// <summary>
/// Madde 16.2 — "İçerik blokları esnek yapıda olacak: metin + görsel, tam genişlik görsel, video
/// embed, akordeon, tab yapısı." Görsel yükleme/silme deseni ProductImage/ReferenceProjectImage ile
/// birebir (uzantı whitelist, MIME çapraz kontrol, magic-byte, GUID dosya adı, telafi mantığı).
/// Sıralama ReferenceProjectImage'daki gibi MoveUp/MoveDown swap ile yönetilir (manuel sayı girişi yok).
/// Her blok bağımsız bir içerik birimidir — Accordion/Tab için ayrı bir grup/panel alt tablosu yoktur
/// (dokümanda tanımlanmayan bir ikinci seviye hiyerarşi icat edilmedi, MVP sınırlaması).
/// Blok tipi değiştiğinde eski tipe ait kullanılmayan veri (görsel/video linki) otomatik temizlenir.
/// </summary>
public class PageContentBlockService
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

    private readonly IPageContentBlockRepository _blockRepository;
    private readonly IPageRepository _pageRepository;
    private readonly ITranslationService _translationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PageContentBlockService> _logger;

    public PageContentBlockService(
        IPageContentBlockRepository blockRepository,
        IPageRepository pageRepository,
        ITranslationService translationService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<PageContentBlockService> logger)
    {
        _blockRepository = blockRepository;
        _pageRepository = pageRepository;
        _translationService = translationService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<PageContentBlockDto>> GetByPageIdAsync(int pageId)
    {
        var blocks = await _blockRepository.GetByPageIdAsync(pageId);

        var result = new List<PageContentBlockDto>();
        foreach (var block in blocks.OrderBy(b => b.DisplayOrder))
        {
            result.Add(await MapToDtoAsync(block));
        }

        return result;
    }

    public async Task<PageContentBlockDto?> GetByIdAsync(int pageId, int blockId)
    {
        var block = await _blockRepository.GetByIdAsync(blockId);
        if (block is null || block.PageId != pageId)
        {
            return null;
        }

        return await MapToDtoAsync(block);
    }

    public async Task<PageContentBlockOperationResult> AddAsync(AddPageContentBlockRequest request)
    {
        var page = await _pageRepository.GetByIdAsync(request.PageId);
        if (page is null)
        {
            return PageContentBlockOperationResult.Failure("Sayfa bulunamadı.");
        }

        var usesImage = UsesImage(request.BlockType);
        var usesVideo = request.BlockType == PageBlockType.VideoEmbed;
        var hasImageUpload = request.ImageLength is > 0 && request.ImageContent is not null;

        if (hasImageUpload && !usesImage)
        {
            return PageContentBlockOperationResult.Failure(
                $"{PageEnumDisplay.GetBlockTypeLabel(request.BlockType)} blok tipinde görsel yüklenemez.");
        }

        var effectiveVideoEmbedUrl = usesVideo ? request.VideoEmbedUrl?.Trim() : null;

        var typeValidation = await ValidateBlockContentAsync(
            request.BlockType, request.Translations, hasImage: hasImageUpload, effectiveVideoEmbedUrl);
        if (typeValidation is not null)
        {
            return PageContentBlockOperationResult.Failure(typeValidation);
        }

        (string? Error, string? Extension) imageValidation = (null, null);
        if (hasImageUpload)
        {
            imageValidation = await ValidateImageAsync(
                request.ImageOriginalFileName!, request.ImageContentType!,
                request.ImageLength!.Value, request.ImageContent!);
            if (imageValidation.Error is not null)
            {
                return PageContentBlockOperationResult.Failure(imageValidation.Error);
            }
        }

        var existingBlocks = await _blockRepository.GetByPageIdAsync(request.PageId);
        if (request.IsActive && request.EnforceExclusiveActivation)
        {
            foreach (var existingBlock in existingBlocks.Where(x => x.IsActive))
            {
                existingBlock.SetActive(false);
            }
        }

        var block = new PageContentBlock(request.PageId, request.BlockType, existingBlocks.Count, effectiveVideoEmbedUrl, request.IsActive);
        await _blockRepository.AddAsync(block);

        // Yeni bloğun Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik +
        // görsel klasör anahtarı Page.Id bazlı olduğu için bu adımda gerekmiyor, ama Translation
        // kaydı için gerekiyor) — Banner/Blog deseniyle aynı iki-SaveChanges akışı.
        await _unitOfWork.SaveChangesAsync();

        if (hasImageUpload)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"pages/{request.PageId}";

            string savedPath;
            try
            {
                savedPath = await _fileStorageService.SaveAsync(folder, request.ImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayfa bloğu görseli diske kaydedilirken hata oluştu. PageId={PageId}, BlockId={BlockId}", request.PageId, block.Id);
                return PageContentBlockOperationResult.Failure("Görsel kaydedilirken bir hata oluştu.");
            }

            block.SetImagePath(savedPath);
        }

        await SaveTranslationsAsync(block.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return PageContentBlockOperationResult.Success();
    }

    public async Task<PageContentBlockOperationResult> UpdateAsync(int pageId, int blockId, UpdatePageContentBlockRequest request)
    {
        var block = await _blockRepository.GetByIdAsync(blockId);
        if (block is null || block.PageId != pageId)
        {
            return PageContentBlockOperationResult.Failure("Blok bulunamadı.");
        }

        var usesImage = UsesImage(request.BlockType);
        var usesVideo = request.BlockType == PageBlockType.VideoEmbed;
        var replacingImage = request.ImageLength is > 0 && request.ImageContent is not null;

        if (replacingImage && !usesImage)
        {
            return PageContentBlockOperationResult.Failure(
                $"{PageEnumDisplay.GetBlockTypeLabel(request.BlockType)} blok tipinde görsel yüklenemez.");
        }

        // Blok tipi görsel kullanmayan bir tipe değiştiriliyorsa (ör. TextImage -> VideoEmbed),
        // mevcut görsel otomatik olarak kaldırılır — kullanıcı ayrıca "görseli kaldır" işaretlemese bile.
        var removingImageDueToTypeChange = !usesImage && block.ImagePath is not null;
        var effectiveRemoveImage = request.RemoveImage || removingImageDueToTypeChange;

        var hasImageAfterUpdate = replacingImage || (!effectiveRemoveImage && block.ImagePath is not null);

        // Blok tipi VideoEmbed değilse video linki otomatik temizlenir (ör. VideoEmbed -> Accordion).
        var effectiveVideoEmbedUrl = usesVideo ? request.VideoEmbedUrl?.Trim() : null;

        var typeValidation = await ValidateBlockContentAsync(
            request.BlockType, request.Translations, hasImage: hasImageAfterUpdate, effectiveVideoEmbedUrl);
        if (typeValidation is not null)
        {
            return PageContentBlockOperationResult.Failure(typeValidation);
        }

        (string? Error, string? Extension) imageValidation = (null, null);
        if (replacingImage)
        {
            imageValidation = await ValidateImageAsync(
                request.ImageOriginalFileName!, request.ImageContentType!,
                request.ImageLength!.Value, request.ImageContent!);
            if (imageValidation.Error is not null)
            {
                return PageContentBlockOperationResult.Failure(imageValidation.Error);
            }
        }

        var oldImagePath = block.ImagePath;
        string? newSavedPath = null;

        if (replacingImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"pages/{pageId}";

            try
            {
                newSavedPath = await _fileStorageService.SaveAsync(folder, request.ImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayfa bloğu görseli değiştirilirken diske kaydetme hatası oluştu. PageId={PageId}, BlockId={BlockId}", pageId, blockId);
                return PageContentBlockOperationResult.Failure("Görsel kaydedilirken bir hata oluştu.");
            }
        }

        block.UpdateBlockType(request.BlockType, effectiveVideoEmbedUrl);
        if (request.IsActive && request.EnforceExclusiveActivation)
        {
            var siblingBlocks = await _blockRepository.GetByPageIdAsync(pageId);
            foreach (var sibling in siblingBlocks.Where(x => x.Id != blockId && x.IsActive))
            {
                sibling.SetActive(false);
            }
        }
        block.SetActive(request.IsActive);

        if (replacingImage)
        {
            block.SetImagePath(newSavedPath);
        }
        else if (effectiveRemoveImage)
        {
            block.SetImagePath(null);
        }

        await SaveTranslationsAsync(block.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        if ((replacingImage || effectiveRemoveImage) && oldImagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(oldImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski sayfa bloğu görseli silinemedi. FilePath={FilePath}", oldImagePath);
            }
        }

        return PageContentBlockOperationResult.Success();
    }

    public Task<PageContentBlockOperationResult> MoveUpAsync(int pageId, int blockId) =>
        MoveAsync(pageId, blockId, moveUp: true);

    public Task<PageContentBlockOperationResult> MoveDownAsync(int pageId, int blockId) =>
        MoveAsync(pageId, blockId, moveUp: false);

    public async Task<PageContentBlockOperationResult> ActivateExclusiveAsync(int pageId, int blockId)
    {
        var blocks = await _blockRepository.GetByPageIdAsync(pageId);
        var selected = blocks.FirstOrDefault(x => x.Id == blockId);
        if (selected is null)
        {
            return PageContentBlockOperationResult.Failure("Kayıt bulunamadı.");
        }

        foreach (var block in blocks)
        {
            block.SetActive(block.Id == blockId);
        }

        await _unitOfWork.SaveChangesAsync();
        return PageContentBlockOperationResult.Success();
    }

    public async Task<PageContentBlockOperationResult> SetActiveAsync(int pageId, int blockId, bool isActive)
    {
        if (isActive)
        {
            return await ActivateExclusiveAsync(pageId, blockId);
        }

        var block = await _blockRepository.GetByIdAsync(blockId);
        if (block is null || block.PageId != pageId)
        {
            return PageContentBlockOperationResult.Failure("Kayıt bulunamadı.");
        }

        block.SetActive(false);
        await _unitOfWork.SaveChangesAsync();
        return PageContentBlockOperationResult.Success();
    }

    public async Task<PageContentBlockOperationResult> DeleteAsync(int pageId, int blockId)
    {
        var block = await _blockRepository.GetByIdAsync(blockId);
        if (block is null || block.PageId != pageId)
        {
            return PageContentBlockOperationResult.Failure("Blok bulunamadı.");
        }

        var imagePath = block.ImagePath;

        await _translationService.DeleteTranslationsForAsync(EntityType.PageContentBlock, blockId);
        _blockRepository.Remove(block);

        await _unitOfWork.SaveChangesAsync();

        if (imagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(imagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blok veritabanından silindi ama görsel silinemedi. FilePath={FilePath}", imagePath);
            }
        }

        return PageContentBlockOperationResult.Success();
    }

    /// <summary>
    /// Sayfa silme akışında (PageService.DeleteAsync) kullanılır — tüm blokların DB kaydını,
    /// Translation'larını ve fiziksel görsellerini temizler. DB cascade (FK) zaten satırları
    /// silecektir; bu metot asıl olarak Translation + fiziksel dosya temizliğini garanti eder
    /// (ReferenceProjectImageService.DeleteAllForReferenceProjectAsync ile aynı desen).
    /// </summary>
    public async Task DeleteAllForPageAsync(int pageId)
    {
        var blocks = await _blockRepository.GetByPageIdAsync(pageId);
        if (blocks.Count == 0)
        {
            return;
        }

        foreach (var block in blocks)
        {
            await _translationService.DeleteTranslationsForAsync(EntityType.PageContentBlock, block.Id);
        }

        _blockRepository.RemoveRange(blocks);
        await _unitOfWork.SaveChangesAsync();

        foreach (var block in blocks)
        {
            if (block.ImagePath is null)
            {
                continue;
            }

            try
            {
                _fileStorageService.Delete(block.ImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayfa silinirken blok görseli silinemedi. FilePath={FilePath}", block.ImagePath);
            }
        }
    }

    private async Task<PageContentBlockOperationResult> MoveAsync(int pageId, int blockId, bool moveUp)
    {
        var blocks = (await _blockRepository.GetByPageIdAsync(pageId))
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Id)
            .ToList();

        var index = blocks.FindIndex(b => b.Id == blockId);
        if (index < 0)
        {
            return PageContentBlockOperationResult.Failure("Blok bulunamadı.");
        }

        var targetIndex = moveUp ? index - 1 : index + 1;
        if (targetIndex < 0 || targetIndex >= blocks.Count)
        {
            return PageContentBlockOperationResult.Success();
        }

        var current = blocks[index];
        var neighbor = blocks[targetIndex];

        var currentOrder = current.DisplayOrder;
        var neighborOrder = neighbor.DisplayOrder;

        current.UpdateDisplayOrder(neighborOrder);
        neighbor.UpdateDisplayOrder(currentOrder);

        await _unitOfWork.SaveChangesAsync();
        return PageContentBlockOperationResult.Success();
    }

    private static bool UsesImage(PageBlockType blockType) =>
        blockType is PageBlockType.TextImage or PageBlockType.FullWidthImage;

    /// <summary>
    /// Madde 16.2'nin 5 blok tipi için minimum içerik kuralı. Her tipin gereksiz alanları zorunlu
    /// tutulmaz — yalnızca o tipin anlamlı bir içerik birimi olabilmesi için zorunlu olan tek alan
    /// doğrulanır (kullanıcı talimatındaki örnek validasyon tablosuna göre).
    /// </summary>
    private async Task<string?> ValidateBlockContentAsync(
        PageBlockType blockType,
        IReadOnlyList<PageContentBlockTranslationInput> translations,
        bool hasImage,
        string? videoEmbedUrl)
    {
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return "Aktif Türkçe (TR) dili bulunamadı.";
        }

        var trInput = translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        var trTitle = trInput?.Title;
        var trContent = trInput?.Content;

        return blockType switch
        {
            PageBlockType.TextImage when string.IsNullOrWhiteSpace(trContent) =>
                "Metin + Görsel blok tipinde Türkçe (TR) içerik metni zorunludur.",
            PageBlockType.FullWidthImage when !hasImage =>
                "Tam Genişlik Görsel blok tipinde görsel zorunludur.",
            PageBlockType.VideoEmbed when string.IsNullOrWhiteSpace(videoEmbedUrl) =>
                "Video Embed blok tipinde video linki zorunludur.",
            PageBlockType.Accordion when string.IsNullOrWhiteSpace(trTitle) =>
                "Akordeon blok tipinde Türkçe (TR) başlık zorunludur.",
            PageBlockType.Tab when string.IsNullOrWhiteSpace(trTitle) =>
                "Sekme (Tab) blok tipinde Türkçe (TR) başlık zorunludur.",
            _ => null
        };
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

    private async Task SaveTranslationsAsync(int blockId, IReadOnlyList<PageContentBlockTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(blockId, input.LanguageId, PageContentBlockFields.Title, input.Title);
            await SaveFieldAsync(blockId, input.LanguageId, PageContentBlockFields.Content, input.Content);
        }
    }

    private async Task SaveFieldAsync(int blockId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.PageContentBlock, blockId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.PageContentBlock, blockId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<PageContentBlockDto> MapToDtoAsync(PageContentBlock block)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.PageContentBlock, block.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new PageContentBlockTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Title = Get(PageContentBlockFields.Title),
                    Content = Get(PageContentBlockFields.Content)
                };
            })
            .ToList();

        return new PageContentBlockDto
        {
            Id = block.Id,
            PageId = block.PageId,
            BlockType = block.BlockType,
            DisplayOrder = block.DisplayOrder,
            ImagePath = block.ImagePath,
            VideoEmbedUrl = block.VideoEmbedUrl,
            IsActive = block.IsActive,
            CreatedAt = block.CreatedAt,
            Translations = translationDtos
        };
    }
}
