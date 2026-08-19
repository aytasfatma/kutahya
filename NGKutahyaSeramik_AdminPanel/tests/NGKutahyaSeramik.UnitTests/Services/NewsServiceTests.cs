using Application.News;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.UnitTests.Services;

public class NewsServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly NewsService _sut;
    private readonly NewsCategoryService _categoryService;

    public NewsServiceTests()
    {
        _sut = new NewsService(
            new NewsRepository(_ctx.DbContext),
            new NewsCategoryRepository(_ctx.DbContext),
            _ctx.Translations,
            _ctx.FileStorage,
            _ctx.UnitOfWork,
            NullLogger<NewsService>.Instance);

        _categoryService = new NewsCategoryService(
            new NewsCategoryRepository(_ctx.DbContext),
            _ctx.Translations,
            _ctx.UnitOfWork);
    }

    public void Dispose() => _ctx.Dispose();

    private static IReadOnlyList<NewsTranslationInput> TrOnly(string title, string? content = null) =>
    [
        new NewsTranslationInput { LanguageId = 1, Title = title, Content = content }
    ];

    [Fact]
    public async Task CreateAsync_WithValidTrTitle_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations = TrOnly("İlk Haber")
        });

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().ContainSingle(n => n.DisplayTitle == "İlk Haber");
    }

    [Fact]
    public async Task CreateAsync_WithoutTrTitle_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations = [new NewsTranslationInput { LanguageId = 2, Title = "English Only" }]
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Türkçe");
    }

    [Fact]
    public async Task CreateAsync_PersistsAllTranslationFields()
    {
        await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations =
            [
                new NewsTranslationInput
                {
                    LanguageId = 1,
                    Title = "Başlık",
                    Content = "İçerik",
                    SeoUrl = "baslik",
                    MetaTitle = "Meta Başlık",
                    MetaDescription = "Meta Açıklama"
                }
            ]
        });

        var news = (await _sut.GetAllAsync()).Single();
        var tr = news.Translations.Single(t => t.LanguageId == 1);

        tr.Title.Should().Be("Başlık");
        tr.Content.Should().Be("İçerik");
        tr.SeoUrl.Should().Be("baslik");
        tr.MetaTitle.Should().Be("Meta Başlık");
        tr.MetaDescription.Should().Be("Meta Açıklama");
    }

    [Fact]
    public async Task UpdateAsync_ClearingOptionalTranslationField_RemovesIt()
    {
        var created = await CreateWithTranslationAsync(title: "Başlık", content: "İçerik");
        var newsId = (await _sut.GetAllAsync()).Single().Id;

        await _sut.UpdateAsync(newsId, new UpdateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations = TrOnly("Başlık", content: null)
        });

        var updated = (await _sut.GetAllAsync()).Single();
        var tr = updated.Translations.Single(t => t.LanguageId == 1);
        tr.Content.Should().BeNull();
        _ctx.Translations.GetValueOrDefault(EntityType.News, newsId, 1, NewsFields.Content).Should().BeNull();
    }

    private async Task<NewsOperationResult> CreateWithTranslationAsync(string title, string? content) =>
        await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations = TrOnly(title, content)
        });

    [Fact]
    public async Task CreateAsync_WithNonExistentCategory_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            NewsCategoryId = 999,
            Status = NewsStatus.Draft,
            Translations = TrOnly("Kategorili Haber")
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("kategorisi");
    }

    [Fact]
    public async Task CreateAsync_WithExistingCategory_Succeeds()
    {
        var categoryResult = await _categoryService.CreateAsync(new CreateNewsCategoryRequest
        {
            DisplayOrder = 1,
            Translations = [new NewsCategoryTranslationInput { LanguageId = 1, Name = "Ödüller" }]
        });
        categoryResult.Succeeded.Should().BeTrue();
        var category = (await _categoryService.GetAllAsync()).Single();

        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            NewsCategoryId = category.Id,
            Status = NewsStatus.Draft,
            Translations = TrOnly("Ödül Haberi")
        });

        result.Succeeded.Should().BeTrue();
        var news = (await _sut.GetAllAsync()).Single();
        news.NewsCategoryId.Should().Be(category.Id);
        news.NewsCategoryName.Should().Be("Ödüller");
    }

    [Fact]
    public async Task CreateAsync_WithoutNewsCategory_SucceedsWithNullCategory()
    {
        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            NewsCategoryId = null,
            Status = NewsStatus.Draft,
            Translations = TrOnly("Kategorisiz Haber")
        });

        result.Succeeded.Should().BeTrue();
        var news = (await _sut.GetAllAsync()).Single();
        news.NewsCategoryId.Should().BeNull();
    }

    [Fact]
    public async Task DeletingNewsCategory_SetsNewsCategoryIdToNull_NewsSurvives()
    {
        var categoryResult = await _categoryService.CreateAsync(new CreateNewsCategoryRequest
        {
            DisplayOrder = 1,
            Translations = [new NewsCategoryTranslationInput { LanguageId = 1, Name = "Sertifikalar" }]
        });
        categoryResult.Succeeded.Should().BeTrue();
        var category = (await _categoryService.GetAllAsync()).Single();

        await _sut.CreateAsync(new CreateNewsRequest
        {
            NewsCategoryId = category.Id,
            Status = NewsStatus.Draft,
            Translations = TrOnly("Sertifika Haberi")
        });

        var deleteResult = await _categoryService.DeleteAsync(category.Id);
        deleteResult.Succeeded.Should().BeTrue();

        var news = (await _sut.GetAllAsync()).Single();
        news.NewsCategoryId.Should().BeNull();
    }

    [Theory]
    [InlineData(NewsStatus.Draft)]
    [InlineData(NewsStatus.Published)]
    [InlineData(NewsStatus.Archived)]
    public async Task CreateAsync_WithEachStatus_PersistsStatus(NewsStatus status)
    {
        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = status,
            Translations = TrOnly($"{status} Haberi")
        });

        result.Succeeded.Should().BeTrue();
        var news = (await _sut.GetAllAsync()).Single(n => n.Status == status);
        news.Status.Should().Be(status);
    }

    [Fact]
    public async Task CreateAsync_WithoutPublishDate_SucceedsWithNullPublishDate()
    {
        var result = await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = NewsStatus.Draft,
            PublishDate = null,
            Translations = TrOnly("Taslak Haber")
        });

        result.Succeeded.Should().BeTrue();
        var news = (await _sut.GetAllAsync()).Single();
        news.PublishDate.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithFeaturedImage_CallsFileStorageServiceSave()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using var _ = content;

        await _sut.CreateAsync(new CreateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations = TrOnly("Görselli Haber"),
            FeaturedImageOriginalFileName = fileName,
            FeaturedImageContentType = contentType,
            FeaturedImageLength = length,
            FeaturedImageContent = content
        });

        _ctx.FileStorage.SaveCalls.Should().ContainSingle();
        var news = (await _sut.GetAllAsync()).Single();
        news.FeaturedImagePath.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReplacingFeaturedImage_DeletesOldFile()
    {
        var (fileName1, contentType1, length1, content1) = ImageUploadFactory.ValidJpeg();
        await using (content1)
        {
            await _sut.CreateAsync(new CreateNewsRequest
            {
                Status = NewsStatus.Draft,
                Translations = TrOnly("Haber"),
                FeaturedImageOriginalFileName = fileName1,
                FeaturedImageContentType = contentType1,
                FeaturedImageLength = length1,
                FeaturedImageContent = content1
            });
        }

        var created = (await _sut.GetAllAsync()).Single();
        var oldPath = created.FeaturedImagePath!;

        var (fileName2, contentType2, length2, content2) = ImageUploadFactory.ValidPng();
        await using (content2)
        {
            var updateResult = await _sut.UpdateAsync(created.Id, new UpdateNewsRequest
            {
                Status = NewsStatus.Draft,
                Translations = TrOnly("Haber"),
                FeaturedImageOriginalFileName = fileName2,
                FeaturedImageContentType = contentType2,
                FeaturedImageLength = length2,
                FeaturedImageContent = content2
            });

            updateResult.Succeeded.Should().BeTrue();
        }

        _ctx.FileStorage.DeleteCalls.Should().Contain(oldPath);
    }

    [Fact]
    public async Task UpdateAsync_RemovingFeaturedImage_CallsDelete()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using (content)
        {
            await _sut.CreateAsync(new CreateNewsRequest
            {
                Status = NewsStatus.Draft,
                Translations = TrOnly("Haber"),
                FeaturedImageOriginalFileName = fileName,
                FeaturedImageContentType = contentType,
                FeaturedImageLength = length,
                FeaturedImageContent = content
            });
        }

        var created = (await _sut.GetAllAsync()).Single();
        var oldPath = created.FeaturedImagePath!;

        await _sut.UpdateAsync(created.Id, new UpdateNewsRequest
        {
            Status = NewsStatus.Draft,
            Translations = TrOnly("Haber"),
            RemoveFeaturedImage = true
        });

        _ctx.FileStorage.DeleteCalls.Should().Contain(oldPath);
        var updated = (await _sut.GetAllAsync()).Single();
        updated.FeaturedImagePath.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTranslationsAndPhysicalFile()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using (content)
        {
            await _sut.CreateAsync(new CreateNewsRequest
            {
                Status = NewsStatus.Draft,
                Translations = TrOnly("Silinecek Haber"),
                FeaturedImageOriginalFileName = fileName,
                FeaturedImageContentType = contentType,
                FeaturedImageLength = length,
                FeaturedImageContent = content
            });
        }

        var created = (await _sut.GetAllAsync()).Single();
        var imagePath = created.FeaturedImagePath!;

        var deleteResult = await _sut.DeleteAsync(created.Id);

        deleteResult.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().BeEmpty();
        _ctx.Translations.HasAnyTranslationsFor(EntityType.News, created.Id).Should().BeFalse();
        _ctx.FileStorage.DeleteCalls.Should().Contain(imagePath);
    }
}
