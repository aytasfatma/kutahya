using Application.Common;

namespace Application.Languages;

/// <summary>
/// Backlog #3 — Dil Yönetimi panel modülü. Madde 17.2'nin listelediği 7 dilin (TR/EN/DE/FR/ES/AR/RU)
/// yönetim ekranı; Language/Translation ALTYAPISI (Task 3.1A/3.1B, ADR-012) zaten kuruluydu, bu
/// yalnızca üzerine ince bir CRUD katmanı ekliyor. Kapsam kasıtlı olarak dar: Index + güvenli Edit.
/// Yeni dil ekleme/silme YOK (dokümanda dayanağı yok, "seed edilmiş diller silinemez" talimatına
/// zaten Create/Delete action'ı hiç var olmayarak uyulmuş oluyor — guard kodu yazmaya bile gerek yok).
/// </summary>
public class LanguageService
{
    private const string TurkishLanguageCode = "TR";

    private readonly ILanguageRepository _languageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LanguageService(ILanguageRepository languageRepository, IUnitOfWork unitOfWork)
    {
        _languageRepository = languageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LanguageDto>> GetAllAsync()
    {
        var languages = await _languageRepository.GetAllAsync();
        return languages
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.Code)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<LanguageDto?> GetByIdAsync(int id)
    {
        var language = await _languageRepository.GetByIdAsync(id);
        return language is null ? null : MapToDto(language);
    }

    public async Task<LanguageOperationResult> UpdateAsync(int id, UpdateLanguageRequest request)
    {
        var language = await _languageRepository.GetByIdAsync(id);
        if (language is null)
        {
            return LanguageOperationResult.Failure("Dil bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return LanguageOperationResult.Failure("Dil adı zorunludur.");
        }

        if (request.DisplayOrder < 1)
        {
            return LanguageOperationResult.Failure(SortOrderValidationMessages.Minimum);
        }

        var languages = await _languageRepository.GetAllAsync();
        if (SortOrderValidation.HasDuplicate(languages, request.DisplayOrder, id, l => l.Id, l => l.DisplayOrder))
        {
            return LanguageOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        // Kesin kural (görev talimatı): "Türkçe devre dışı bırakılamaz" — TR, projedeki tüm
        // zorunlu-çeviri iş kurallarının (Product/Category/Collection/... ValidateAsync) varsaydığı
        // varsayılan dil; TR pasif olursa bu servisler anlamlı bir hata bile veremeden bozulurdu.
        if (string.Equals(language.Code, TurkishLanguageCode, StringComparison.OrdinalIgnoreCase) && !request.IsActive)
        {
            return LanguageOperationResult.Failure("Türkçe (TR) devre dışı bırakılamaz.");
        }

        language.UpdateDetails(request.Name.Trim(), request.DisplayOrder);

        if (request.IsActive != language.IsActive)
        {
            if (request.IsActive)
            {
                language.Activate();
            }
            else
            {
                language.Deactivate();
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return LanguageOperationResult.Success();
    }

    private static LanguageDto MapToDto(Domain.Entities.Language language) => new()
    {
        Id = language.Id,
        Code = language.Code,
        Name = language.Name,
        IsActive = language.IsActive,
        DisplayOrder = language.DisplayOrder
    };
}


