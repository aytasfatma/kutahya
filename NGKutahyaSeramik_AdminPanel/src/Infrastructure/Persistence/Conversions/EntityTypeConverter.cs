using Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.Conversions;

public class EntityTypeConverter : ValueConverter<EntityType, string>
{
    public EntityTypeConverter()
        : base(
            v => EntityTypeMapping.ToDbValue(v),
            v => EntityTypeMapping.FromDbValue(v))
    {
    }
}
