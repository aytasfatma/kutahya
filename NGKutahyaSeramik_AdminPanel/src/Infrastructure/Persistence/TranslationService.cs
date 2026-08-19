using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class TranslationService : ITranslationService
{
    private readonly AppDbContext _dbContext;

    public TranslationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TranslationValue>> GetTranslationsAsync(EntityType entityType, int entityId)
    {
        var rows = await (
            from t in _dbContext.Translations.AsNoTracking()
            join l in _dbContext.Languages.AsNoTracking() on t.LanguageId equals l.Id
            where t.EntityType == entityType && t.EntityId == entityId
            select new TranslationValue(t.LanguageId, l.Code, t.FieldName, t.Value))
            .ToListAsync();

        return rows;
    }

    public async Task SetTranslationAsync(EntityType entityType, int entityId, int languageId, string fieldName, string value)
    {
        var existing = await _dbContext.Translations.FirstOrDefaultAsync(t =>
            t.EntityType == entityType &&
            t.EntityId == entityId &&
            t.LanguageId == languageId &&
            t.FieldName == fieldName);

        if (existing is null)
        {
            var translation = new Translation(entityType, entityId, languageId, fieldName, value);
            await _dbContext.Translations.AddAsync(translation);
        }
        else
        {
            existing.UpdateValue(value);
        }
    }

    public async Task DeleteTranslationFieldAsync(EntityType entityType, int entityId, int languageId, string fieldName)
    {
        var existing = await _dbContext.Translations.FirstOrDefaultAsync(t =>
            t.EntityType == entityType &&
            t.EntityId == entityId &&
            t.LanguageId == languageId &&
            t.FieldName == fieldName);

        if (existing is not null)
        {
            _dbContext.Translations.Remove(existing);
        }
    }

    public async Task DeleteTranslationsForAsync(EntityType entityType, int entityId)
    {
        var existing = await _dbContext.Translations
            .Where(t => t.EntityType == entityType && t.EntityId == entityId)
            .ToListAsync();

        _dbContext.Translations.RemoveRange(existing);
    }

    public async Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync()
    {
        var languages = await _dbContext.Languages
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new LanguageInfo(l.Id, l.Code, l.Name))
            .ToListAsync();

        return languages;
    }
}
