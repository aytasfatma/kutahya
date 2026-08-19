using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Conversions;

namespace NGKutahyaSeramik.UnitTests.Persistence;

public class EntityTypeMappingTests
{
    [Fact]
    public void Surface_RoundTripsThroughDatabaseMapping()
    {
        var storedValue = EntityTypeMapping.ToDbValue(EntityType.Surface);

        storedValue.Should().Be("SURFACE");
        EntityTypeMapping.FromDbValue(storedValue).Should().Be(EntityType.Surface);
    }
}
