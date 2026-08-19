using Domain.Enums;

namespace Application.Seo;

public class SeoContentTypeDto
{
    public EntityType EntityType { get; init; }
    public string Label { get; init; } = string.Empty;
    public bool SupportsMetaFields { get; init; }
}

public class SeoRecordSummaryDto
{
    public EntityType EntityType { get; init; }
    public int EntityId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}

public class SeoLanguageFieldDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class SeoRecordDetailDto
{
    public EntityType EntityType { get; init; }
    public int EntityId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool SupportsMetaFields { get; init; }
    public IReadOnlyList<SeoLanguageFieldDto> Languages { get; init; } = [];
}

/// <summary>"Eksik SEO alanlarını listeleme" raporu — bir kayıt+dil kombinasyonunda SeoUrl (veya
/// desteklenen tipler için MetaTitle/MetaDescription) boşsa bir satır üretir.</summary>
public class SeoMissingFieldDto
{
    public EntityType EntityType { get; init; }
    public int EntityId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public IReadOnlyList<string> MissingFields { get; init; } = [];
}

/// <summary>"Duplicate SEO URL kontrolü" — EntityType+Dil bazında normalize edilmiş SeoUrl'ü aynı olan
/// birden fazla kayıt varsa bir grup üretir (kullanıcının onayladığı kapsam: DB constraint değil,
/// uygulama-seviyesi rapor/uyarı).</summary>
public class SeoDuplicateGroupDto
{
    public EntityType EntityType { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string NormalizedSeoUrl { get; init; } = string.Empty;
    public IReadOnlyList<SeoRecordSummaryDto> Records { get; init; } = [];
}
