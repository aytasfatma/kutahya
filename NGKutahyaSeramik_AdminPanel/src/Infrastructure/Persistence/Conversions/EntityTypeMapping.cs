using Domain.Enums;

namespace Infrastructure.Persistence.Conversions;

public static class EntityTypeMapping
{
    private static readonly (EntityType Enum, string Value)[] Map =
    [
        (EntityType.Product, "PRODUCT"),
        (EntityType.Category, "CATEGORY"),
        (EntityType.Collection, "COLLECTION"),
        (EntityType.Blog, "BLOG"),
        (EntityType.News, "NEWS"),
        (EntityType.Page, "PAGE"),
        (EntityType.Banner, "BANNER"),
        (EntityType.ReferenceProject, "REFERENCE_PROJECT"),
        (EntityType.Dealer, "DEALER"),
        (EntityType.BlogCategory, "BLOG_CATEGORY"),
        (EntityType.NewsCategory, "NEWS_CATEGORY"),
        (EntityType.PageContentBlock, "PAGE_CONTENT_BLOCK"),
        (EntityType.Surface, "SURFACE")
    ];

    private static readonly Dictionary<EntityType, string> ToDb =
        Map.ToDictionary(m => m.Enum, m => m.Value);

    private static readonly Dictionary<string, EntityType> FromDb =
        Map.ToDictionary(m => m.Value, m => m.Enum);

    public static string ToDbValue(EntityType value)
    {
        if (ToDb.TryGetValue(value, out var dbValue))
        {
            return dbValue;
        }

        throw new NotSupportedException($"EntityType '{value}' için DB mapping tanımlı değil.");
    }

    public static EntityType FromDbValue(string value)
    {
        if (FromDb.TryGetValue(value, out var enumValue))
        {
            return enumValue;
        }

        throw new InvalidOperationException(
            $"Veritabanındaki EntityType değeri '{value}' tanınmıyor. Enum/mapping senkronizasyonu kontrol edilmeli.");
    }
}
