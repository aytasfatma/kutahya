using Application.Products;
using Application.Storage;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.ProductImages;

public class ProductImageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    public const int MaxImagesPerProduct = 6;

    private static readonly IReadOnlyDictionary<string, string> AllowedExtensionContentTypes =
        new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductImageService> _logger;

    public ProductImageService(
        IProductImageRepository productImageRepository,
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<ProductImageService> logger)
    {
        _productImageRepository = productImageRepository;
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductImageDto>> GetByProductIdAsync(int productId)
    {
        var images = await _productImageRepository.GetByProductIdAsync(productId);
        return images
            .OrderBy(i => i.DisplayOrder)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ProductImageOperationResult> ValidateUploadAsync(AddProductImageRequest request)
    {
        if (request.Length <= 0)
        {
            return ProductImageOperationResult.Failure("Bos dosya yuklenemez.");
        }

        if (request.Length > MaxFileSizeBytes)
        {
            return ProductImageOperationResult.Failure(
                $"Dosya boyutu {MaxFileSizeBytes / (1024 * 1024)} MB sinirini asiyor.");
        }

        var extension = Path.GetExtension(request.OriginalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensionContentTypes.TryGetValue(extension, out var expectedContentType))
        {
            return ProductImageOperationResult.Failure(
                "Izin verilmeyen dosya uzantisi. Yalnizca .jpg, .jpeg, .png, .webp kabul edilir.");
        }

        if (!string.Equals(request.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return ProductImageOperationResult.Failure("Dosya uzantisi ile icerik tipi (MIME) uyusmuyor.");
        }

        if (!request.Content.CanSeek)
        {
            return ProductImageOperationResult.Failure("Dosya akisi okunamadi.");
        }

        var header = new byte[12];
        request.Content.Position = 0;
        var bytesRead = await request.Content.ReadAsync(header.AsMemory(0, header.Length));
        request.Content.Position = 0;

        if (!HasValidImageSignature(header, bytesRead, extension))
        {
            return ProductImageOperationResult.Failure("Dosya icerigi belirtilen gorsel formatiyla uyusmuyor.");
        }

        return ProductImageOperationResult.Success();
    }

    public async Task<ProductImageOperationResult> AddImageAsync(AddProductImageRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return ProductImageOperationResult.Failure("Urun bulunamadi.");
        }

        var validation = await ValidateUploadAsync(request);
        if (!validation.Succeeded)
        {
            return validation;
        }

        request.Content.Position = 0;

        var existingImages = await _productImageRepository.GetByProductIdAsync(request.ProductId);
        if (existingImages.Count >= MaxImagesPerProduct)
        {
            return ProductImageOperationResult.Failure($"Bir ürüne en fazla {MaxImagesPerProduct} görsel eklenebilir.");
        }
        var isFirstImage = existingImages.Count == 0;

        var extension = Path.GetExtension(request.OriginalFileName)!.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var folder = $"products/{product.ProductCode}/{request.ImageType.ToString().ToLowerInvariant()}";

        string savedPath;
        try
        {
            savedPath = await _fileStorageService.SaveAsync(folder, request.Content, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Urun gorseli diske kaydedilirken hata olustu. ProductId={ProductId}", request.ProductId);
            return ProductImageOperationResult.Failure("Dosya kaydedilirken bir hata olustu.");
        }

        try
        {
            var image = new ProductImage(request.ProductId, request.ImageType, savedPath, isFirstImage, existingImages.Count);
            await _productImageRepository.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Urun gorseli veritabanina kaydedilirken hata olustu, fiziksel dosya geri aliniyor. ProductId={ProductId}",
                request.ProductId);
            _fileStorageService.Delete(savedPath);
            return ProductImageOperationResult.Failure("Gorsel kaydedilirken bir hata olustu.");
        }

        return ProductImageOperationResult.Success();
    }

    public async Task<ProductImageOperationResult> SetPrimaryAsync(int productId, int imageId)
    {
        var image = await _productImageRepository.GetByIdAsync(imageId);
        if (image is null || image.ProductId != productId)
        {
            return ProductImageOperationResult.Failure("Gorsel bulunamadi.");
        }

        if (image.IsPrimary)
        {
            return ProductImageOperationResult.Success();
        }

        var productImages = await _productImageRepository.GetByProductIdAsync(productId);
        var currentPrimary = productImages.FirstOrDefault(i => i.IsPrimary);
        currentPrimary?.SetPrimary(false);

        image.SetPrimary(true);

        await _unitOfWork.SaveChangesAsync();
        return ProductImageOperationResult.Success();
    }

    public Task<ProductImageOperationResult> MoveUpAsync(int productId, int imageId) =>
        MoveAsync(productId, imageId, moveUp: true);

    public Task<ProductImageOperationResult> MoveDownAsync(int productId, int imageId) =>
        MoveAsync(productId, imageId, moveUp: false);

    public async Task<ProductImageOperationResult> DeleteImageAsync(int productId, int imageId)
    {
        var image = await _productImageRepository.GetByIdAsync(imageId);
        if (image is null || image.ProductId != productId)
        {
            return ProductImageOperationResult.Failure("Gorsel bulunamadi.");
        }

        var wasPrimary = image.IsPrimary;
        var filePath = image.FilePath;

        _productImageRepository.Remove(image);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            _fileStorageService.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gorsel veritabanindan silindi ama fiziksel dosya silinemedi. FilePath={FilePath}", filePath);
        }

        if (wasPrimary)
        {
            var remaining = await _productImageRepository.GetByProductIdAsync(productId);
            var next = remaining.OrderBy(i => i.DisplayOrder).FirstOrDefault();
            if (next is not null)
            {
                next.SetPrimary(true);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return ProductImageOperationResult.Success();
    }

    public async Task DeleteAllForProductAsync(int productId)
    {
        var images = await _productImageRepository.GetByProductIdAsync(productId);
        if (images.Count == 0)
        {
            return;
        }

        _productImageRepository.RemoveRange(images);
        await _unitOfWork.SaveChangesAsync();

        foreach (var image in images)
        {
            try
            {
                _fileStorageService.Delete(image.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Urun silinirken gorsel dosyasi silinemedi. FilePath={FilePath}", image.FilePath);
            }
        }
    }

    private async Task<ProductImageOperationResult> MoveAsync(int productId, int imageId, bool moveUp)
    {
        var images = (await _productImageRepository.GetByProductIdAsync(productId))
            .OrderBy(i => i.DisplayOrder)
            .ThenBy(i => i.Id)
            .ToList();

        var index = images.FindIndex(i => i.Id == imageId);
        if (index < 0)
        {
            return ProductImageOperationResult.Failure("Gorsel bulunamadi.");
        }

        var targetIndex = moveUp ? index - 1 : index + 1;
        if (targetIndex < 0 || targetIndex >= images.Count)
        {
            return ProductImageOperationResult.Success();
        }

        var current = images[index];
        var neighbor = images[targetIndex];

        var currentOrder = current.DisplayOrder;
        var neighborOrder = neighbor.DisplayOrder;

        current.UpdateDisplayOrder(neighborOrder);
        neighbor.UpdateDisplayOrder(currentOrder);

        await _unitOfWork.SaveChangesAsync();
        return ProductImageOperationResult.Success();
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

    private static ProductImageDto MapToDto(ProductImage image) => new()
    {
        Id = image.Id,
        ProductId = image.ProductId,
        ImageType = image.ImageType,
        FilePath = image.FilePath,
        IsPrimary = image.IsPrimary,
        DisplayOrder = image.DisplayOrder
    };
}
