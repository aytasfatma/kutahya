using Domain.Entities;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class CollectionFactory
{
    public static Collection CreateValid(string? imagePath = null, int displayOrder = 0) =>
        new(imagePath, displayOrder);
}
