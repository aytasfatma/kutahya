namespace Application.Banners;

public class BannerTranslationInput
{
    public int LanguageId { get; init; }
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? ButtonText { get; init; }
    public string? ButtonUrl { get; init; }
}

public class BannerRequestBase
{
    public DateTime? PublishStartDate { get; init; }
    public DateTime? PublishEndDate { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<BannerTranslationInput> Translations { get; init; } = [];

    /// <summary>Görsel tamamen opsiyoneldir (Madde 16.1'de Zorunluluk bilgisi yok) — Content null ise yükleme yapılmaz.</summary>
    public string? ImageOriginalFileName { get; init; }
    public string? ImageContentType { get; init; }
    public long? ImageLength { get; init; }
    public Stream? ImageContent { get; init; }
}

public class CreateBannerRequest : BannerRequestBase
{
}

public class UpdateBannerRequest : BannerRequestBase
{
    /// <summary>Yeni görsel yüklenmeden mevcut görselin kaldırılması istendiğinde true.</summary>
    public bool RemoveImage { get; init; }
}
