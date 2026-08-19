using Domain.Enums;

namespace Domain.Entities;

public class Translation
{
    public int Id { get; private set; }
    public EntityType EntityType { get; private set; }
    public int EntityId { get; private set; }
    public int LanguageId { get; private set; }
    public string FieldName { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    public Language Language { get; private set; } = null!;

    private Translation()
    {
    }

    public Translation(EntityType entityType, int entityId, int languageId, string fieldName, string value)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("FieldName boş veya whitespace olamaz.", nameof(fieldName));
        }

        EntityType = entityType;
        EntityId = entityId;
        LanguageId = languageId;
        FieldName = fieldName;
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void UpdateValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}
