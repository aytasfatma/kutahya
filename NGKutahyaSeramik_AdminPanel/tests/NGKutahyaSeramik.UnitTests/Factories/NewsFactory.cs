using Domain.Entities;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class NewsFactory
{
    public static News CreateDraft(int? newsCategoryId = null) =>
        new(newsCategoryId, publishDate: null, NewsStatus.Draft);

    public static News CreatePublished(int? newsCategoryId = null, DateTime? publishDate = null) =>
        new(newsCategoryId, publishDate ?? DateTime.UtcNow, NewsStatus.Published);
}

public static class NewsCategoryFactory
{
    public static NewsCategory CreateValid(int displayOrder = 0) => new(displayOrder);
}
