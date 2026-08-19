using Application.Blogs;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.UnitTests.Services;

public class BlogServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly BlogService _sut;

    public BlogServiceTests()
    {
        _sut = new BlogService(
            new BlogRepository(_ctx.DbContext),
            new BlogCategoryRepository(_ctx.DbContext),
            new TagRepository(_ctx.DbContext),
            _ctx.Translations,
            _ctx.FileStorage,
            _ctx.UnitOfWork,
            NullLogger<BlogService>.Instance);
    }

    public void Dispose() => _ctx.Dispose();

    private static IReadOnlyList<BlogTranslationInput> TrOnly(string title, string? excerpt = null) =>
    [
        new BlogTranslationInput { LanguageId = 1, Title = title, Excerpt = excerpt }
    ];

    [Fact]
    public async Task CreateAsync_WithValidTrTitle_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = TrOnly("İlk Blog Yazısı")
        });

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().ContainSingle(b => b.DisplayTitle == "İlk Blog Yazısı");
    }

    [Fact]
    public async Task CreateAsync_WithoutTrTitle_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = [new BlogTranslationInput { LanguageId = 2, Title = "English Only" }]
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Türkçe");
    }

    [Fact]
    public async Task CreateAsync_PersistsAllTranslationFields()
    {
        await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations =
            [
                new BlogTranslationInput
                {
                    LanguageId = 1,
                    Title = "Başlık",
                    Excerpt = "Özet",
                    Content = "İçerik",
                    SeoUrl = "baslik",
                    MetaTitle = "Meta Başlık",
                    MetaDescription = "Meta Açıklama"
                }
            ]
        });

        var blog = (await _sut.GetAllAsync()).Single();
        var tr = blog.Translations.Single(t => t.LanguageId == 1);

        tr.Title.Should().Be("Başlık");
        tr.Excerpt.Should().Be("Özet");
        tr.Content.Should().Be("İçerik");
        tr.SeoUrl.Should().Be("baslik");
        tr.MetaTitle.Should().Be("Meta Başlık");
        tr.MetaDescription.Should().Be("Meta Açıklama");
    }

    [Fact]
    public async Task CreateAsync_WithNewTagNames_CreatesTagsAndAssociatesThem()
    {
        await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = TrOnly("Etiketli Yazı"),
            TagNames = ["Banyo", "Mutfak"]
        });

        var blog = (await _sut.GetAllAsync()).Single();
        blog.Tags.Should().BeEquivalentTo(["Banyo", "Mutfak"]);
    }

    [Fact]
    public async Task CreateAsync_WithCaseVariantOfExistingTag_ReusesSameTag_NoDuplicate()
    {
        await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = TrOnly("İlk Yazı"),
            TagNames = ["Banyo"]
        });

        await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = TrOnly("İkinci Yazı"),
            TagNames = ["banyo"]
        });

        var allTags = await _ctx.DbContext.Tags.ToListAsync();
        allTags.Should().ContainSingle(t => string.Equals(t.Name, "Banyo", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateAsync_WithFeaturedImage_CallsFileStorageServiceSave()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using var _ = content;

        await _sut.CreateAsync(new CreateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = TrOnly("Görselli Yazı"),
            FeaturedImageOriginalFileName = fileName,
            FeaturedImageContentType = contentType,
            FeaturedImageLength = length,
            FeaturedImageContent = content
        });

        _ctx.FileStorage.SaveCalls.Should().ContainSingle();
        var blog = (await _sut.GetAllAsync()).Single();
        blog.FeaturedImagePath.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReplacingFeaturedImage_DeletesOldFile()
    {
        var (fileName1, contentType1, length1, content1) = ImageUploadFactory.ValidJpeg();
        await using (content1)
        {
            await _sut.CreateAsync(new CreateBlogRequest
            {
                Status = BlogStatus.Draft,
                Translations = TrOnly("Yazı"),
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
            var updateResult = await _sut.UpdateAsync(created.Id, new UpdateBlogRequest
            {
                Status = BlogStatus.Draft,
                Translations = TrOnly("Yazı"),
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
            await _sut.CreateAsync(new CreateBlogRequest
            {
                Status = BlogStatus.Draft,
                Translations = TrOnly("Yazı"),
                FeaturedImageOriginalFileName = fileName,
                FeaturedImageContentType = contentType,
                FeaturedImageLength = length,
                FeaturedImageContent = content
            });
        }

        var created = (await _sut.GetAllAsync()).Single();
        var oldPath = created.FeaturedImagePath!;

        await _sut.UpdateAsync(created.Id, new UpdateBlogRequest
        {
            Status = BlogStatus.Draft,
            Translations = TrOnly("Yazı"),
            RemoveFeaturedImage = true
        });

        _ctx.FileStorage.DeleteCalls.Should().Contain(oldPath);
        var updated = (await _sut.GetAllAsync()).Single();
        updated.FeaturedImagePath.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithoutBlogCategory_SucceedsWithNullCategory()
    {
        var result = await _sut.CreateAsync(new CreateBlogRequest
        {
            BlogCategoryId = null,
            Status = BlogStatus.Draft,
            Translations = TrOnly("Kategorisiz Yazı")
        });

        result.Succeeded.Should().BeTrue();
        var blog = (await _sut.GetAllAsync()).Single();
        blog.BlogCategoryId.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTranslationsAndPhysicalFile()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using (content)
        {
            await _sut.CreateAsync(new CreateBlogRequest
            {
                Status = BlogStatus.Draft,
                Translations = TrOnly("Silinecek Yazı"),
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
        _ctx.Translations.HasAnyTranslationsFor(EntityType.Blog, created.Id).Should().BeFalse();
        _ctx.FileStorage.DeleteCalls.Should().Contain(imagePath);
    }
}
