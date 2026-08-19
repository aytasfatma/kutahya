using Domain.Entities;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class BlogFactory
{
    public static Blog CreateDraft(int? blogCategoryId = null, string? author = "Test Yazar") =>
        new(blogCategoryId, author, publishDate: null, BlogStatus.Draft);

    public static Blog CreatePublished(int? blogCategoryId = null, DateTime? publishDate = null, string? author = "Test Yazar") =>
        new(blogCategoryId, author, publishDate ?? DateTime.UtcNow, BlogStatus.Published);
}

public static class BlogCategoryFactory
{
    public static BlogCategory CreateValid(int displayOrder = 0) => new(displayOrder);
}
