using Domain.Entities;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class CategoryFactory
{
    public static Category CreateRoot(string? imagePath = null, int displayOrder = 0) =>
        new(parentCategoryId: null, imagePath, displayOrder);

    public static Category CreateChild(int parentCategoryId, string? imagePath = null, int displayOrder = 0) =>
        new(parentCategoryId, imagePath, displayOrder);
}
