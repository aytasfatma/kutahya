using Application.Translations;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Mocks;

/// <summary>
/// ITranslationService'in gerçek DB'siz, bellek-içi bir uygulaması. Gerçek `TranslationService` ile
/// aynı sözleşmeyi (upsert/delete/list) uygular — testler böylece BlogService/PageService/ProductService
/// gibi tüketicilerin gerçek upsert/silme akışını (yalnızca persistence katmanı sahte) doğrulayabilir.
/// </summary>
public class FakeTranslationService : ITranslationService
{
    private readonly List<LanguageInfo> _languages;
    private readonly Dictionary<(EntityType EntityType, int EntityId, int LanguageId, string FieldName), string> _store = new();

    public FakeTranslationService(IEnumerable<LanguageInfo>? languages = null)
    {
        _languages = languages?.ToList() ??
        [
            new LanguageInfo(1, "TR", "Türkçe"),
            new LanguageInfo(2, "EN", "English")
        ];
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        Task.FromResult<IReadOnlyList<LanguageInfo>>(_languages);

    public Task<IReadOnlyList<TranslationValue>> GetTranslationsAsync(EntityType entityType, int entityId)
    {
        var results = _store
            .Where(kv => kv.Key.EntityType == entityType && kv.Key.EntityId == entityId)
            .Select(kv => new TranslationValue(kv.Key.LanguageId, LanguageCodeFor(kv.Key.LanguageId), kv.Key.FieldName, kv.Value))
            .ToList();

        return Task.FromResult<IReadOnlyList<TranslationValue>>(results);
    }

    public Task SetTranslationAsync(EntityType entityType, int entityId, int languageId, string fieldName, string value)
    {
        _store[(entityType, entityId, languageId, fieldName)] = value;
        return Task.CompletedTask;
    }

    public Task DeleteTranslationFieldAsync(EntityType entityType, int entityId, int languageId, string fieldName)
    {
        _store.Remove((entityType, entityId, languageId, fieldName));
        return Task.CompletedTask;
    }

    public Task DeleteTranslationsForAsync(EntityType entityType, int entityId)
    {
        foreach (var key in _store.Keys.Where(k => k.EntityType == entityType && k.EntityId == entityId).ToList())
        {
            _store.Remove(key);
        }

        return Task.CompletedTask;
    }

    /// <summary>Testlerde "silme sonrası hiçbir çeviri kalmadı" iddiasını kısaca ifade etmek için.</summary>
    public bool HasAnyTranslationsFor(EntityType entityType, int entityId) =>
        _store.Keys.Any(k => k.EntityType == entityType && k.EntityId == entityId);

    public string? GetValueOrDefault(EntityType entityType, int entityId, int languageId, string fieldName) =>
        _store.GetValueOrDefault((entityType, entityId, languageId, fieldName));

    private string LanguageCodeFor(int languageId) =>
        _languages.FirstOrDefault(l => l.Id == languageId)?.Code ?? "??";
}
