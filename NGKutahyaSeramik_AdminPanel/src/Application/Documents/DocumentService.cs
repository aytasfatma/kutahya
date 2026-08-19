using Application.Collections;
using Application.Common;
using Application.Products;
using Application.Storage;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Documents;

public class DocumentService
{
    private const string TrLanguageCode = "TR";
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;
    private const string AllowedExtension = ".pdf";
    private const string AllowedContentType = "application/pdf";
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    private readonly IDocumentRepository _documentRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ITranslationService _translationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IProductRepository productRepository,
        ICollectionRepository collectionRepository,
        ITranslationService translationService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _productRepository = productRepository;
        _collectionRepository = collectionRepository;
        _translationService = translationService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<(int Id, string Label)>> GetProductOptionsAsync()
    {
        return await _productRepository.GetProductCodeOptionsAsync();
    }

    public async Task<IReadOnlyList<(int Id, string Label)>> GetCollectionOptionsAsync()
    {
        var collections = await _collectionRepository.GetAllAsync();
        var result = new List<(int Id, string Label)>();

        foreach (var collection in collections.OrderBy(c => c.DisplayOrder))
        {
            var translations = await _translationService.GetTranslationsAsync(EntityType.Collection, collection.Id);
            var name = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == CollectionFields.Name)?.Value;

            result.Add((collection.Id, name ?? $"#{collection.Id}"));
        }

        return result;
    }

    public async Task<IReadOnlyList<DocumentDto>> GetAllAsync()
    {
        var documents = await _documentRepository.GetAllAsync();

        var result = new List<DocumentDto>();
        foreach (var document in documents)
        {
            result.Add(await MapToDtoAsync(document));
        }

        return result;
    }

    public async Task<int> GetNextDisplayOrderAsync(DocumentType documentType, int languageId)
    {
        var documents = await _documentRepository.GetAllAsync();
        return SortOrderValidation.Next(
            documents.Where(d => d.DocumentType == documentType && d.LanguageId == languageId),
            d => d.DisplayOrder);
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        return document is null ? null : await MapToDtoAsync(document);
    }

    public async Task<DocumentOperationResult> CreateAsync(CreateDocumentRequest request)
    {
        var metadataValidation = await ValidateMetadataAsync(
            request.Title, request.DocumentType, request.LanguageId, request.DisplayOrder, request.ProductIds, request.CollectionIds, excludeDocumentId: null);
        if (metadataValidation is not null)
        {
            return DocumentOperationResult.Failure(metadataValidation);
        }

        var fileValidation = await ValidateFileAsync(request.OriginalFileName, request.ContentType, request.Length, request.Content);
        if (fileValidation.Error is not null)
        {
            return DocumentOperationResult.Failure(fileValidation.Error);
        }

        var folder = $"documents/{DocumentEnumDisplay.GetFolderSegment(request.DocumentType)}";
        var fileName = $"{Guid.NewGuid():N}{fileValidation.Extension}";

        string savedPath;
        try
        {
            savedPath = await _fileStorageService.SaveAsync(folder, request.Content, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman diske kaydedilirken hata oluştu.");
            return DocumentOperationResult.Failure("Dosya kaydedilirken bir hata oluştu.");
        }

        try
        {
            var document = new Document(
                request.DocumentType,
                request.Brand,
                request.Title.Trim(),
                string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                request.LanguageId,
                savedPath,
                request.OriginalFileName,
                fileValidation.Extension!,
                request.ContentType,
                request.Length,
                request.DisplayOrder);

            await _documentRepository.AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            await _documentRepository.ReplaceProductRelationsAsync(document.Id, request.ProductIds.Distinct().ToList());
            await _documentRepository.ReplaceCollectionRelationsAsync(document.Id, request.CollectionIds.Distinct().ToList());
            await _unitOfWork.SaveChangesAsync();
        }
        catch (SortOrderConflictException)
        {
            _fileStorageService.Delete(savedPath);
            return DocumentOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman veritabanına kaydedilirken hata oluştu, fiziksel dosya geri alınıyor.");
            _fileStorageService.Delete(savedPath);
            return DocumentOperationResult.Failure("Doküman kaydedilirken bir hata oluştu.");
        }

        return DocumentOperationResult.Success();
    }

    public async Task<DocumentOperationResult> UpdateAsync(int id, UpdateDocumentRequest request)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document is null)
        {
            return DocumentOperationResult.Failure("Doküman bulunamadı.");
        }

        var metadataValidation = await ValidateMetadataAsync(
            request.Title, request.DocumentType, request.LanguageId, request.DisplayOrder, request.ProductIds, request.CollectionIds, excludeDocumentId: id);
        if (metadataValidation is not null)
        {
            return DocumentOperationResult.Failure(metadataValidation);
        }

        var replacingFile = request.Content is not null && request.Length is > 0;
        string? newSavedPath = null;
        string? newExtension = null;

        if (replacingFile)
        {
            var fileValidation = await ValidateFileAsync(request.OriginalFileName!, request.ContentType!, request.Length!.Value, request.Content!);
            if (fileValidation.Error is not null)
            {
                return DocumentOperationResult.Failure(fileValidation.Error);
            }

            newExtension = fileValidation.Extension;
            var folder = $"documents/{DocumentEnumDisplay.GetFolderSegment(request.DocumentType)}";
            var fileName = $"{Guid.NewGuid():N}{newExtension}";

            try
            {
                newSavedPath = await _fileStorageService.SaveAsync(folder, request.Content!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doküman dosyası değiştirilirken diske kaydetme hatası oluştu. DocumentId={DocumentId}", id);
                return DocumentOperationResult.Failure("Dosya kaydedilirken bir hata oluştu.");
            }
        }

        var oldFilePath = document.FilePath;

        try
        {
            document.UpdateMetadata(
                request.DocumentType,
                request.Brand,
                request.Title.Trim(),
                string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                request.LanguageId,
                request.DisplayOrder);

            if (replacingFile)
            {
                document.UpdateFile(newSavedPath!, request.OriginalFileName!, newExtension!, request.ContentType!, request.Length!.Value);
            }

            await _documentRepository.ReplaceProductRelationsAsync(id, request.ProductIds.Distinct().ToList());
            await _documentRepository.ReplaceCollectionRelationsAsync(id, request.CollectionIds.Distinct().ToList());
            await _unitOfWork.SaveChangesAsync();
        }
        catch (SortOrderConflictException)
        {
            if (newSavedPath is not null)
            {
                _fileStorageService.Delete(newSavedPath);
            }

            return DocumentOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman güncellenirken hata oluştu. DocumentId={DocumentId}", id);
            if (newSavedPath is not null)
            {
                _fileStorageService.Delete(newSavedPath);
            }

            return DocumentOperationResult.Failure("Doküman güncellenirken bir hata oluştu.");
        }

        if (replacingFile)
        {
            try
            {
                _fileStorageService.Delete(oldFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski doküman dosyası silinemedi. FilePath={FilePath}", oldFilePath);
            }
        }

        return DocumentOperationResult.Success();
    }

    public async Task<DocumentOperationResult> ToggleActiveAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document is null)
        {
            return DocumentOperationResult.Failure("Doküman bulunamadı.");
        }

        if (document.IsActive)
        {
            document.Deactivate();
        }
        else
        {
            document.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return DocumentOperationResult.Success();
    }

    public async Task<DocumentOperationResult> DeleteAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document is null)
        {
            return DocumentOperationResult.Failure("Doküman bulunamadı.");
        }

        var filePath = document.FilePath;

        // Ürün/koleksiyon ilişki satırları DB'de Cascade FK ile otomatik silinir (bkz. DocumentConfiguration).
        _documentRepository.Remove(document);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            _fileStorageService.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman veritabanından silindi ama fiziksel dosya silinemedi. FilePath={FilePath}", filePath);
        }

        return DocumentOperationResult.Success();
    }

    private async Task<string?> ValidateMetadataAsync(
        string title,
        DocumentType documentType,
        int languageId,
        int displayOrder,
        IReadOnlyList<int> productIds,
        IReadOnlyList<int> collectionIds,
        int? excludeDocumentId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Doküman adı (Title) zorunludur.";
        }

        if (displayOrder < 1)
        {
            return SortOrderValidationMessages.Minimum;
        }

        var documents = await _documentRepository.GetAllAsync();
        var sameScope = documents.Where(d => d.DocumentType == documentType && d.LanguageId == languageId);
        if (SortOrderValidation.HasDuplicate(sameScope, displayOrder, excludeDocumentId, d => d.Id, d => d.DisplayOrder))
        {
            return SortOrderValidationMessages.Duplicate;
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        if (activeLanguages.All(l => l.Id != languageId))
        {
            return "Seçilen dil aktif diller arasında bulunamadı.";
        }

        foreach (var productId in productIds.Distinct())
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is null)
            {
                return $"Seçilen ürün (Id={productId}) bulunamadı.";
            }
        }

        foreach (var collectionId in collectionIds.Distinct())
        {
            var collection = await _collectionRepository.GetByIdAsync(collectionId);
            if (collection is null)
            {
                return $"Seçilen koleksiyon (Id={collectionId}) bulunamadı.";
            }
        }

        return null;
    }

    private async Task<(string? Error, string? Extension)> ValidateFileAsync(
        string originalFileName, string contentType, long length, Stream content)
    {
        if (length <= 0)
        {
            return ("Boş dosya yüklenemez.", null);
        }

        if (length > MaxFileSizeBytes)
        {
            return ($"Dosya boyutu {MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.", null);
        }

        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (extension != AllowedExtension)
        {
            return ("İzin verilmeyen dosya uzantısı. Yalnızca .pdf kabul edilir.", null);
        }

        if (!string.Equals(contentType, AllowedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return ("Dosya uzantısı ile içerik tipi (MIME) uyuşmuyor.", null);
        }

        if (!content.CanSeek)
        {
            return ("Dosya akışı okunamadı.", null);
        }

        var header = new byte[PdfSignature.Length];
        content.Position = 0;
        var bytesRead = await content.ReadAsync(header.AsMemory(0, header.Length));
        content.Position = 0;

        if (bytesRead < PdfSignature.Length || !header.AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature))
        {
            return ("Dosya içeriği geçerli bir PDF imzasıyla başlamıyor.", null);
        }

        return (null, extension);
    }

    private async Task<DocumentDto> MapToDtoAsync(Document document)
    {
        var languages = await _translationService.GetActiveLanguagesAsync();
        var language = languages.FirstOrDefault(l => l.Id == document.LanguageId);

        var relatedProductIds = await _documentRepository.GetRelatedProductIdsAsync(document.Id);
        var relatedProducts = new List<DocumentRelatedItemDto>();
        foreach (var productId in relatedProductIds)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is not null)
            {
                relatedProducts.Add(new DocumentRelatedItemDto { Id = product.Id, Label = product.ProductCode });
            }
        }

        var relatedCollectionIds = await _documentRepository.GetRelatedCollectionIdsAsync(document.Id);
        var relatedCollections = new List<DocumentRelatedItemDto>();
        foreach (var collectionId in relatedCollectionIds)
        {
            var collectionTranslations = await _translationService.GetTranslationsAsync(EntityType.Collection, collectionId);
            var name = collectionTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == CollectionFields.Name)?.Value;

            relatedCollections.Add(new DocumentRelatedItemDto { Id = collectionId, Label = name ?? $"#{collectionId}" });
        }

        return new DocumentDto
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            Brand = document.Brand,
            Title = document.Title,
            Description = document.Description,
            LanguageId = document.LanguageId,
            LanguageCode = language?.Code ?? string.Empty,
            LanguageName = language?.Name ?? string.Empty,
            FilePath = document.FilePath,
            OriginalFileName = document.OriginalFileName,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            DisplayOrder = document.DisplayOrder,
            IsActive = document.IsActive,
            RelatedProducts = relatedProducts,
            RelatedCollections = relatedCollections
        };
    }
}

