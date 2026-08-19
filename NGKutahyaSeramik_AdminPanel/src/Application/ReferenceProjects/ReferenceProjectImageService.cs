using Application.Storage;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.ReferenceProjects;

/// <summary>
/// ProductImageService (Task 5.1/ADR-013) ile birebir aynı desen: boyut/uzantı/MIME/magic-byte
/// doğrulama, "ilk yükleme otomatik kapak" otomasyonu, silme fallback'i, başarısız DB yazımında
/// fiziksel dosya geri alma. Tek fark: ImageType boyutu yok (Madde 23.1 galeri+kapak, tip ayrımı
/// istemiyor) ve klasör anahtarı ProductCode değil ReferenceProjectId (ADR-014'teki gibi, doğal bir
/// iş kodu olmadığı için surrogate Id kullanıldı).
/// </summary>
public class ReferenceProjectImageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedExtensionContentTypes =
        new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly IReferenceProjectImageRepository _referenceProjectImageRepository;
    private readonly IReferenceProjectRepository _referenceProjectRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReferenceProjectImageService> _logger;

    public ReferenceProjectImageService(
        IReferenceProjectImageRepository referenceProjectImageRepository,
        IReferenceProjectRepository referenceProjectRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<ReferenceProjectImageService> logger)
    {
        _referenceProjectImageRepository = referenceProjectImageRepository;
        _referenceProjectRepository = referenceProjectRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReferenceProjectImageDto>> GetByReferenceProjectIdAsync(int referenceProjectId)
    {
        var images = await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(referenceProjectId);
        return images
            .OrderBy(i => i.DisplayOrder)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ReferenceProjectImageOperationResult> AddImageAsync(AddReferenceProjectImageRequest request)
    {
        var referenceProject = await _referenceProjectRepository.GetByIdAsync(request.ReferenceProjectId);
        if (referenceProject is null)
        {
            return ReferenceProjectImageOperationResult.Failure("Referans proje bulunamadı.");
        }

        if (request.Length <= 0)
        {
            return ReferenceProjectImageOperationResult.Failure("Boş dosya yüklenemez.");
        }

        if (request.Length > MaxFileSizeBytes)
        {
            return ReferenceProjectImageOperationResult.Failure(
                $"Dosya boyutu {MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.");
        }

        var extension = Path.GetExtension(request.OriginalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensionContentTypes.TryGetValue(extension, out var expectedContentType))
        {
            return ReferenceProjectImageOperationResult.Failure(
                "İzin verilmeyen dosya uzantısı. Yalnızca .jpg, .jpeg, .png, .webp kabul edilir.");
        }

        if (!string.Equals(request.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return ReferenceProjectImageOperationResult.Failure("Dosya uzantısı ile içerik tipi (MIME) uyuşmuyor.");
        }

        if (!request.Content.CanSeek)
        {
            return ReferenceProjectImageOperationResult.Failure("Dosya akışı okunamadı.");
        }

        var header = new byte[12];
        request.Content.Position = 0;
        var bytesRead = await request.Content.ReadAsync(header.AsMemory(0, header.Length));
        request.Content.Position = 0;

        if (!HasValidImageSignature(header, bytesRead, extension))
        {
            return ReferenceProjectImageOperationResult.Failure("Dosya içeriği belirtilen görsel formatıyla uyuşmuyor.");
        }

        var existingImages = await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(request.ReferenceProjectId);
        var isFirstImage = existingImages.Count == 0;

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var folder = $"projects/{request.ReferenceProjectId}";

        string savedPath;
        try
        {
            savedPath = await _fileStorageService.SaveAsync(folder, request.Content, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Referans proje görseli diske kaydedilirken hata oluştu. ReferenceProjectId={ReferenceProjectId}", request.ReferenceProjectId);
            return ReferenceProjectImageOperationResult.Failure("Dosya kaydedilirken bir hata oluştu.");
        }

        try
        {
            var image = new ReferenceProjectImage(request.ReferenceProjectId, savedPath, isFirstImage, existingImages.Count);
            await _referenceProjectImageRepository.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Referans proje görseli veritabanına kaydedilirken hata oluştu, fiziksel dosya geri alınıyor. ReferenceProjectId={ReferenceProjectId}",
                request.ReferenceProjectId);
            _fileStorageService.Delete(savedPath);
            return ReferenceProjectImageOperationResult.Failure("Görsel kaydedilirken bir hata oluştu.");
        }

        return ReferenceProjectImageOperationResult.Success();
    }

    public async Task<ReferenceProjectImageOperationResult> SetFeaturedAsync(int referenceProjectId, int imageId)
    {
        var image = await _referenceProjectImageRepository.GetByIdAsync(imageId);
        if (image is null || image.ReferenceProjectId != referenceProjectId)
        {
            return ReferenceProjectImageOperationResult.Failure("Görsel bulunamadı.");
        }

        if (image.IsFeatured)
        {
            return ReferenceProjectImageOperationResult.Success();
        }

        var images = await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(referenceProjectId);
        var currentFeatured = images.FirstOrDefault(i => i.IsFeatured);
        currentFeatured?.SetFeatured(false);

        image.SetFeatured(true);

        await _unitOfWork.SaveChangesAsync();
        return ReferenceProjectImageOperationResult.Success();
    }

    public Task<ReferenceProjectImageOperationResult> MoveUpAsync(int referenceProjectId, int imageId) =>
        MoveAsync(referenceProjectId, imageId, moveUp: true);

    public Task<ReferenceProjectImageOperationResult> MoveDownAsync(int referenceProjectId, int imageId) =>
        MoveAsync(referenceProjectId, imageId, moveUp: false);

    public async Task<ReferenceProjectImageOperationResult> DeleteImageAsync(int referenceProjectId, int imageId)
    {
        var image = await _referenceProjectImageRepository.GetByIdAsync(imageId);
        if (image is null || image.ReferenceProjectId != referenceProjectId)
        {
            return ReferenceProjectImageOperationResult.Failure("Görsel bulunamadı.");
        }

        var wasFeatured = image.IsFeatured;
        var filePath = image.FilePath;

        _referenceProjectImageRepository.Remove(image);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            _fileStorageService.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görsel veritabanından silindi ama fiziksel dosya silinemedi. FilePath={FilePath}", filePath);
        }

        if (wasFeatured)
        {
            var remaining = await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(referenceProjectId);
            var next = remaining.OrderBy(i => i.DisplayOrder).FirstOrDefault();
            if (next is not null)
            {
                next.SetFeatured(true);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return ReferenceProjectImageOperationResult.Success();
    }

    /// <summary>
    /// Referans proje silme akışında (ReferenceProjectService.DeleteAsync) kullanılır — tüm görsel
    /// kayıtlarını ve fiziksel dosyalarını temizler. DB cascade (FK) zaten kayıtları silecektir; bu
    /// metot asıl olarak fiziksel dosya temizliğini garanti eder (ProductImageService ile aynı desen).
    /// </summary>
    public async Task DeleteAllForReferenceProjectAsync(int referenceProjectId)
    {
        var images = await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(referenceProjectId);
        if (images.Count == 0)
        {
            return;
        }

        _referenceProjectImageRepository.RemoveRange(images);
        await _unitOfWork.SaveChangesAsync();

        foreach (var image in images)
        {
            try
            {
                _fileStorageService.Delete(image.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans proje silinirken görsel dosyası silinemedi. FilePath={FilePath}", image.FilePath);
            }
        }
    }

    private async Task<ReferenceProjectImageOperationResult> MoveAsync(int referenceProjectId, int imageId, bool moveUp)
    {
        var images = (await _referenceProjectImageRepository.GetByReferenceProjectIdAsync(referenceProjectId))
            .OrderBy(i => i.DisplayOrder)
            .ThenBy(i => i.Id)
            .ToList();

        var index = images.FindIndex(i => i.Id == imageId);
        if (index < 0)
        {
            return ReferenceProjectImageOperationResult.Failure("Görsel bulunamadı.");
        }

        var targetIndex = moveUp ? index - 1 : index + 1;
        if (targetIndex < 0 || targetIndex >= images.Count)
        {
            return ReferenceProjectImageOperationResult.Success();
        }

        var current = images[index];
        var neighbor = images[targetIndex];

        var currentOrder = current.DisplayOrder;
        var neighborOrder = neighbor.DisplayOrder;

        current.UpdateDisplayOrder(neighborOrder);
        neighbor.UpdateDisplayOrder(currentOrder);

        await _unitOfWork.SaveChangesAsync();
        return ReferenceProjectImageOperationResult.Success();
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

    private static ReferenceProjectImageDto MapToDto(ReferenceProjectImage image) => new()
    {
        Id = image.Id,
        ReferenceProjectId = image.ReferenceProjectId,
        FilePath = image.FilePath,
        IsFeatured = image.IsFeatured,
        DisplayOrder = image.DisplayOrder
    };
}
