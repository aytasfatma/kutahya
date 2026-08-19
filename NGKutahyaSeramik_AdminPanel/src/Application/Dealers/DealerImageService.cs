using Application.Storage;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Dealers;

public sealed class DealerImageService
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes = new Dictionary<string, string>
    {
        [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".webp"] = "image/webp"
    };

    private readonly IDealerImageRepository _images;
    private readonly IDealerRepository _dealers;
    private readonly IFileStorageService _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DealerImageService> _logger;

    public DealerImageService(IDealerImageRepository images, IDealerRepository dealers, IFileStorageService storage,
        IUnitOfWork unitOfWork, ILogger<DealerImageService> logger)
    {
        _images = images; _dealers = dealers; _storage = storage; _unitOfWork = unitOfWork; _logger = logger;
    }

    public async Task<IReadOnlyList<DealerImageDto>> GetByDealerIdAsync(int dealerId) =>
        (await _images.GetByDealerIdAsync(dealerId)).Select(Map).ToList();

    public async Task<DealerOperationResult> AddAsync(AddDealerImageRequest request)
    {
        if (await _dealers.GetByIdAsync(request.DealerId) is null) return DealerOperationResult.Failure("Satış noktası bulunamadı.");
        if (request.Length <= 0) return DealerOperationResult.Failure("Boş dosya yüklenemez.");
        if (request.Length > MaxFileSize) return DealerOperationResult.Failure("Görsel en fazla 5 MB olabilir.");
        var extension = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(extension, out var expected)) return DealerOperationResult.Failure("Yalnızca JPG, PNG ve WEBP yüklenebilir.");
        if (!string.Equals(request.ContentType, expected, StringComparison.OrdinalIgnoreCase)) return DealerOperationResult.Failure("Dosya uzantısı ile içerik tipi uyuşmuyor.");
        if (!request.Content.CanSeek) return DealerOperationResult.Failure("Dosya akışı okunamadı.");
        var header = new byte[12]; request.Content.Position = 0; var read = await request.Content.ReadAsync(header); request.Content.Position = 0;
        if (!ValidSignature(header, read, extension)) return DealerOperationResult.Failure("Dosya gerçek bir görsel değil.");

        var existing = await _images.GetByDealerIdAsync(request.DealerId);
        string path;
        try { path = await _storage.SaveAsync($"dealers/{request.DealerId}", request.Content, $"{Guid.NewGuid():N}{extension}"); }
        catch (Exception ex) { _logger.LogError(ex, "Bayi görseli kaydedilemedi. DealerId={DealerId}", request.DealerId); return DealerOperationResult.Failure("Görsel kaydedilemedi."); }
        try
        {
            await _images.AddAsync(new DealerImage(request.DealerId, path, existing.Count == 0, existing.Count));
            await _unitOfWork.SaveChangesAsync();
            return DealerOperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bayi görsel kaydı oluşturulamadı. DealerId={DealerId}", request.DealerId);
            _storage.Delete(path); return DealerOperationResult.Failure("Görsel kaydı oluşturulamadı.");
        }
    }

    public async Task<DealerOperationResult> SetFeaturedAsync(int dealerId, int imageId)
    {
        var image = await _images.GetByIdAsync(imageId);
        if (image is null || image.DealerId != dealerId) return DealerOperationResult.Failure("Görsel bulunamadı.");
        if (image.IsFeatured) return DealerOperationResult.Success();
        foreach (var item in await _images.GetByDealerIdAsync(dealerId)) item.SetFeatured(item.Id == imageId);
        await _unitOfWork.SaveChangesAsync(); return DealerOperationResult.Success();
    }

    public Task<DealerOperationResult> MoveUpAsync(int dealerId, int imageId) => MoveAsync(dealerId, imageId, -1);
    public Task<DealerOperationResult> MoveDownAsync(int dealerId, int imageId) => MoveAsync(dealerId, imageId, 1);

    private async Task<DealerOperationResult> MoveAsync(int dealerId, int imageId, int direction)
    {
        var list = (await _images.GetByDealerIdAsync(dealerId)).ToList(); var index = list.FindIndex(x => x.Id == imageId);
        var target = index + direction; if (index < 0) return DealerOperationResult.Failure("Görsel bulunamadı.");
        if (target < 0 || target >= list.Count) return DealerOperationResult.Success();
        var order = list[index].DisplayOrder; list[index].UpdateDisplayOrder(list[target].DisplayOrder); list[target].UpdateDisplayOrder(order);
        await _unitOfWork.SaveChangesAsync(); return DealerOperationResult.Success();
    }

    public async Task<DealerOperationResult> DeleteAsync(int dealerId, int imageId)
    {
        var image = await _images.GetByIdAsync(imageId);
        if (image is null || image.DealerId != dealerId) return DealerOperationResult.Failure("Görsel bulunamadı.");
        var wasFeatured = image.IsFeatured; var path = image.FilePath; _images.Remove(image); await _unitOfWork.SaveChangesAsync();
        try { _storage.Delete(path); } catch (Exception ex) { _logger.LogError(ex, "Bayi görsel dosyası silinemedi. Path={Path}", path); }
        if (wasFeatured) { var next = (await _images.GetByDealerIdAsync(dealerId)).FirstOrDefault(); if (next is not null) { next.SetFeatured(true); await _unitOfWork.SaveChangesAsync(); } }
        return DealerOperationResult.Success();
    }

    public async Task DeleteAllAsync(int dealerId)
    {
        var list = await _images.GetByDealerIdAsync(dealerId); if (list.Count == 0) return;
        _images.RemoveRange(list); await _unitOfWork.SaveChangesAsync();
        foreach (var image in list) try { _storage.Delete(image.FilePath); } catch (Exception ex) { _logger.LogError(ex, "Bayi görseli silinemedi. Path={Path}", image.FilePath); }
    }

    private static bool ValidSignature(byte[] h, int n, string ext) => ext switch
    {
        ".jpg" or ".jpeg" => n >= 3 && h[0] == 0xff && h[1] == 0xd8 && h[2] == 0xff,
        ".png" => n >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4e && h[3] == 0x47 && h[4] == 0x0d && h[5] == 0x0a && h[6] == 0x1a && h[7] == 0x0a,
        ".webp" => n >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P',
        _ => false
    };

    private static DealerImageDto Map(DealerImage x) => new(x.Id, x.DealerId, x.FilePath, x.IsFeatured, x.DisplayOrder);
}
