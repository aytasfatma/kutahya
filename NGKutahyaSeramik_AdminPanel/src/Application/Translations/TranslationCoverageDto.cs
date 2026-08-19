using Domain.Enums;

namespace Application.Translations;

/// <summary>
/// Madde ADR-007'nin daha önce ertelenen kısmı ("Bu fazda Yönetim Paneli fallback uygulamayacak;
/// yalnızca eksik çevirileri gösterecek...") — bu görev tam olarak bu taahhüdü hayata geçiriyor.
/// Tek bir (EntityType, EntityId, Dil, FieldName) satırı — Translation tablosunda o kombinasyon için
/// satır yok VEYA Value boş/whitespace. Fallback/başka dilden doldurma YOK (görev talimatı) — bu
/// DTO yalnızca tespit sonucu, hiçbir zaman bir "görüntülenecek değer" taşımaz.
/// </summary>
public class MissingTranslationDto
{
    public EntityType EntityType { get; init; }
    public string ModuleLabel { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string FieldName { get; init; } = string.Empty;
    public int? ParentEntityId { get; init; }
}

/// <summary>"Dile göre eksik sayısı" — aktif dil listesindeki HER dil (0 dahil) görünür, yalnızca
/// eksiği olanlar değil (Dashboard kart deseniyle tutarlı, sayaç aniden kaybolmaz).</summary>
public class TranslationCoverageByLanguageDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public int MissingCount { get; init; }
}

/// <summary>"Modüle göre eksik sayısı" — desteklenen HER modül (0 dahil) görünür.</summary>
public class TranslationCoverageByModuleDto
{
    public EntityType EntityType { get; init; }
    public string ModuleLabel { get; init; } = string.Empty;
    public int MissingCount { get; init; }
}

public class TranslationCoverageReportDto
{
    public int TotalMissing { get; init; }
    public int TotalRequiredFields { get; init; }
    public int CompletedTranslations { get; init; }
    public decimal CompletionRate { get; init; }
    public IReadOnlyList<TranslationCoverageByLanguageDto> ByLanguage { get; init; } = [];
    public IReadOnlyList<TranslationCoverageByModuleDto> ByModule { get; init; } = [];
    public IReadOnlyList<MissingTranslationDto> Items { get; init; } = [];
}
