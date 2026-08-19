using Domain.Enums;

namespace Application.Translations;

/// <summary>
/// Eksik çeviri listesinden (EntityType, EntityId, Dil, FieldName) toplama/gruplama — saf, DB'siz
/// dönüştürücü (ProductImportReportBuilder ile aynı desen: girdi elle kurulmuş bir liste, DB/dosya
/// gerekmez, bu yüzden ayrı ve hızlı birim testlenebilir).
/// </summary>
public static class TranslationCoverageReportBuilder
{
    public static TranslationCoverageReportDto Build(
        IReadOnlyList<MissingTranslationDto> items,
        IReadOnlyList<LanguageInfo> activeLanguages,
        IReadOnlyList<(EntityType Type, string Label)> supportedTypes,
        int totalRequiredFields = 0)
    {
        var byLanguage = activeLanguages
            .Select(language => new TranslationCoverageByLanguageDto
            {
                LanguageId = language.Id,
                LanguageCode = language.Code,
                LanguageName = language.Name,
                MissingCount = items.Count(i => i.LanguageId == language.Id)
            })
            .ToList();

        var byModule = supportedTypes
            .Select(type => new TranslationCoverageByModuleDto
            {
                EntityType = type.Type,
                ModuleLabel = type.Label,
                MissingCount = items.Count(i => i.EntityType == type.Type)
            })
            .ToList();

        return new TranslationCoverageReportDto
        {
            TotalMissing = items.Count,
            TotalRequiredFields = totalRequiredFields,
            CompletedTranslations = Math.Max(0, totalRequiredFields - items.Count),
            CompletionRate = totalRequiredFields == 0
                ? 100
                : Math.Round((totalRequiredFields - items.Count) * 100m / totalRequiredFields, 1),
            ByLanguage = byLanguage,
            ByModule = byModule,
            Items = items
        };
    }
}
