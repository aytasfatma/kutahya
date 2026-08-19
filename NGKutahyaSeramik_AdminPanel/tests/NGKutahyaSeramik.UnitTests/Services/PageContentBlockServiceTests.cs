using Application.Pages;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.UnitTests.Services;

public class PageContentBlockServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly PageService _pageService;
    private readonly PageContentBlockService _sut;
    private readonly int _pageId;

    public PageContentBlockServiceTests()
    {
        var pageRepository = new PageRepository(_ctx.DbContext);
        var blockRepository = new PageContentBlockRepository(_ctx.DbContext);

        _sut = new PageContentBlockService(
            blockRepository, pageRepository, _ctx.Translations, _ctx.FileStorage,
            _ctx.UnitOfWork, NullLogger<PageContentBlockService>.Instance);

        _pageService = new PageService(pageRepository, _ctx.Translations, _sut, _ctx.UnitOfWork);

        _pageService.CreateAsync(new CreatePageRequest
        {
            Translations = [new PageTranslationInput { LanguageId = 1, Title = "Test Sayfası" }]
        }).GetAwaiter().GetResult();

        _pageId = _pageService.GetAllAsync().GetAwaiter().GetResult().Single().Id;
    }

    public void Dispose() => _ctx.Dispose();

    private static IReadOnlyList<PageContentBlockTranslationInput> Title(string title) =>
    [
        new PageContentBlockTranslationInput { LanguageId = 1, Title = title }
    ];

    private static IReadOnlyList<PageContentBlockTranslationInput> Content(string content) =>
    [
        new PageContentBlockTranslationInput { LanguageId = 1, Content = content }
    ];

    [Fact]
    public async Task AddAsync_TextImage_WithContent_Succeeds()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.TextImage,
            Translations = Content("Metin içeriği")
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_TextImage_WithoutContent_IsRejected()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.TextImage,
            Translations = []
        });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_FullWidthImage_WithoutImage_IsRejected()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.FullWidthImage,
            Translations = []
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("görsel");
    }

    [Fact]
    public async Task AddAsync_FullWidthImage_WithImage_Succeeds()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using var _ = content;

        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.FullWidthImage,
            Translations = [],
            ImageOriginalFileName = fileName,
            ImageContentType = contentType,
            ImageLength = length,
            ImageContent = content
        });

        result.Succeeded.Should().BeTrue();
        _ctx.FileStorage.SaveCalls.Should().ContainSingle();
    }

    [Fact]
    public async Task AddAsync_VideoEmbed_WithoutUrl_IsRejected()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.VideoEmbed,
            Translations = []
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("video");
    }

    [Fact]
    public async Task AddAsync_VideoEmbed_WithUrl_Succeeds()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = "https://youtube.com/embed/abc",
            Translations = []
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_VideoEmbed_WithImage_IsRejected_NoUploadOccurs()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using var _ = content;

        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = "https://youtube.com/embed/abc",
            Translations = [],
            ImageOriginalFileName = fileName,
            ImageContentType = contentType,
            ImageLength = length,
            ImageContent = content
        });

        result.Succeeded.Should().BeFalse();
        _ctx.FileStorage.SaveCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_Accordion_WithoutTitle_IsRejected()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.Accordion,
            Translations = Content("İçerik var ama başlık yok")
        });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_Accordion_WithTitle_Succeeds()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.Accordion,
            Translations = Title("Sıkça Sorulan Soru")
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_Tab_WithTitle_Succeeds()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.Tab,
            Translations = Title("Sekme Başlığı")
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_ToNonExistentPage_IsRejected()
    {
        var result = await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = 999_999,
            BlockType = PageBlockType.Tab,
            Translations = Title("Sekme")
        });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ChangingTypeAwayFromImage_AutomaticallyRemovesOldImage()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidJpeg();
        await using (content)
        {
            await _sut.AddAsync(new AddPageContentBlockRequest
            {
                PageId = _pageId,
                BlockType = PageBlockType.FullWidthImage,
                Translations = [],
                ImageOriginalFileName = fileName,
                ImageContentType = contentType,
                ImageLength = length,
                ImageContent = content
            });
        }

        var block = (await _sut.GetByPageIdAsync(_pageId)).Single();
        var oldImagePath = block.ImagePath!;

        var updateResult = await _sut.UpdateAsync(_pageId, block.Id, new UpdatePageContentBlockRequest
        {
            BlockType = PageBlockType.Accordion,
            Translations = Title("Artık akordeon")
        });

        updateResult.Succeeded.Should().BeTrue();
        var updated = (await _sut.GetByPageIdAsync(_pageId)).Single();
        updated.ImagePath.Should().BeNull();
        _ctx.FileStorage.DeleteCalls.Should().Contain(oldImagePath);
    }

    [Fact]
    public async Task UpdateAsync_ChangingTypeAwayFromVideoEmbed_ClearsVideoUrl()
    {
        await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId,
            BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = "https://youtube.com/embed/xyz",
            Translations = []
        });

        var block = (await _sut.GetByPageIdAsync(_pageId)).Single();
        block.VideoEmbedUrl.Should().NotBeNullOrEmpty();

        var updateResult = await _sut.UpdateAsync(_pageId, block.Id, new UpdatePageContentBlockRequest
        {
            BlockType = PageBlockType.Tab,
            Translations = Title("Artık sekme")
        });

        updateResult.Succeeded.Should().BeTrue();
        var updated = (await _sut.GetByPageIdAsync(_pageId)).Single();
        updated.VideoEmbedUrl.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_RemoveImage_DeletesPhysicalFile()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidPng();
        await using (content)
        {
            await _sut.AddAsync(new AddPageContentBlockRequest
            {
                PageId = _pageId,
                BlockType = PageBlockType.TextImage,
                Translations = Content("İçerik"),
                ImageOriginalFileName = fileName,
                ImageContentType = contentType,
                ImageLength = length,
                ImageContent = content
            });
        }

        var block = (await _sut.GetByPageIdAsync(_pageId)).Single();
        var imagePath = block.ImagePath!;

        var updateResult = await _sut.UpdateAsync(_pageId, block.Id, new UpdatePageContentBlockRequest
        {
            BlockType = PageBlockType.TextImage,
            Translations = Content("İçerik"),
            RemoveImage = true
        });

        updateResult.Succeeded.Should().BeTrue();
        _ctx.FileStorage.DeleteCalls.Should().Contain(imagePath);
    }

    [Fact]
    public async Task MoveUpAndMoveDown_SwapDisplayOrder()
    {
        await _sut.AddAsync(new AddPageContentBlockRequest { PageId = _pageId, BlockType = PageBlockType.Tab, Translations = Title("Birinci") });
        await _sut.AddAsync(new AddPageContentBlockRequest { PageId = _pageId, BlockType = PageBlockType.Tab, Translations = Title("İkinci") });

        var blocks = await _sut.GetByPageIdAsync(_pageId);
        var first = blocks.OrderBy(b => b.DisplayOrder).First();
        var second = blocks.OrderBy(b => b.DisplayOrder).Last();

        var moveResult = await _sut.MoveUpAsync(_pageId, second.Id);
        moveResult.Succeeded.Should().BeTrue();

        var afterMove = await _sut.GetByPageIdAsync(_pageId);
        afterMove.First(b => b.Id == second.Id).DisplayOrder.Should().Be(first.DisplayOrder);
        afterMove.First(b => b.Id == first.Id).DisplayOrder.Should().Be(second.DisplayOrder);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBlockAndPhysicalFile()
    {
        var (fileName, contentType, length, content) = ImageUploadFactory.ValidWebp();
        await using (content)
        {
            await _sut.AddAsync(new AddPageContentBlockRequest
            {
                PageId = _pageId,
                BlockType = PageBlockType.FullWidthImage,
                Translations = [],
                ImageOriginalFileName = fileName,
                ImageContentType = contentType,
                ImageLength = length,
                ImageContent = content
            });
        }

        var block = (await _sut.GetByPageIdAsync(_pageId)).Single();

        var deleteResult = await _sut.DeleteAsync(_pageId, block.Id);

        deleteResult.Succeeded.Should().BeTrue();
        (await _sut.GetByPageIdAsync(_pageId)).Should().BeEmpty();
        _ctx.FileStorage.DeleteCalls.Should().Contain(block.ImagePath!);
    }

    [Fact]
    public async Task SetActiveAsync_ActivatingOneRecord_DeactivatesOtherRecords()
    {
        await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId, BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = "https://youtube.com/embed/first", IsActive = true, Translations = []
        });
        await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId, BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = "https://youtube.com/embed/second", IsActive = false, Translations = []
        });

        var blocks = await _sut.GetByPageIdAsync(_pageId);
        var second = blocks.Single(x => x.VideoEmbedUrl!.EndsWith("second"));
        var result = await _sut.SetActiveAsync(_pageId, second.Id, true);

        result.Succeeded.Should().BeTrue();
        var updated = await _sut.GetByPageIdAsync(_pageId);
        updated.Should().ContainSingle(x => x.IsActive && x.Id == second.Id);
        updated.Should().OnlyContain(x => x.Id == second.Id || !x.IsActive);
    }

    [Fact]
    public async Task SetActiveAsync_PassiveSelection_LeavesRecordPassive()
    {
        await _sut.AddAsync(new AddPageContentBlockRequest
        {
            PageId = _pageId, BlockType = PageBlockType.VideoEmbed,
            VideoEmbedUrl = "https://youtube.com/embed/active", IsActive = true, Translations = []
        });
        var block = (await _sut.GetByPageIdAsync(_pageId)).Single();

        var result = await _sut.SetActiveAsync(_pageId, block.Id, false);

        result.Succeeded.Should().BeTrue();
        (await _sut.GetByPageIdAsync(_pageId)).Single().IsActive.Should().BeFalse();
    }
}
