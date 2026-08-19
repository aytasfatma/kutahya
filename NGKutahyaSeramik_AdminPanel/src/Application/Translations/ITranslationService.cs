using Domain.Enums;

namespace Application.Translations;

public record TranslationValue(int LanguageId, string LanguageCode, string FieldName, string Value);

public record LanguageInfo(int Id, string Code, string Name);

public interface ITranslationService
{
    Task<IReadOnlyList<TranslationValue>> GetTranslationsAsync(EntityType entityType, int entityId);

    Task SetTranslationAsync(EntityType entityType, int entityId, int languageId, string fieldName, string value);

    Task DeleteTranslationFieldAsync(EntityType entityType, int entityId, int languageId, string fieldName);

    Task DeleteTranslationsForAsync(EntityType entityType, int entityId);

    Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync();
}
