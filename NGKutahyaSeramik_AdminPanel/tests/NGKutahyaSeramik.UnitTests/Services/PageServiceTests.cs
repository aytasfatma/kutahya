using Application.Pages;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.UnitTests.Services;

public class PageServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly PageService _sut;
    private readonly PageContentBlockService _blockService;

    public PageServiceTests()
    {
        var pageRepository = new PageRepository(_ctx.DbContext);
        var blockRepository = new PageContentBlockRepository(_ctx.DbContext);

        _blockService = new PageContentBlockService(
            blockRepository, pageRepository, _ctx.Translations, _ctx.FileStorage,
            _ctx.UnitOfWork, NullLogger<PageContentBlockService>.Instance);

        _sut = new PageService(pageRepository, _ctx.Translations, _blockService, _ctx.UnitOfWork);
    }

    public void Dispose() => _ctx.Dispose();

    private static IReadOnlyList<PageTranslationInput> TrOnly(string title) =>
    [
        new PageTranslationInput { LanguageId = 1, Title = title }
    ];

    [Fact]
    public async Task CreateAsync_WithValidTrTitle_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreatePageRequest { Translations = TrOnly("Hakkımızda") });

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().ContainSingle(p => p.DisplayTitle == "Hakkımızda");
    }

    [Fact]
    public async Task CreateAsync_WithoutTrTitle_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreatePageRequest
        {
            Translations = [new PageTranslationInput { LanguageId = 2, Title = "About" }]
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Türkçe");
    }

    [Fact]
    public async Task CreateAsync_PersistsSeoFields()
    {
        await _sut.CreateAsync(new CreatePageRequest
        {
            Translations =
            [
                new PageTranslationInput
                {
                    LanguageId = 1,
                    Title = "Kariyer",
                    SeoUrl = "kariyer",
                    MetaTitle = "Kariyer Fırsatları",
                    MetaDescription = "Açıklama"
                }
            ]
        });

        var page = (await _sut.GetAllAsync()).Single();
        var tr = page.Translations.Single(t => t.LanguageId == 1);

        tr.SeoUrl.Should().Be("kariyer");
        tr.MetaTitle.Should().Be("Kariyer Fırsatları");
        tr.MetaDescription.Should().Be("Açıklama");
    }

    [Fact]
    public async Task UpdateAsync_UpsertsAndClearsBlankOptionalFields()
    {
        await _sut.CreateAsync(new CreatePageRequest
        {
            Translations = [new PageTranslationInput { LanguageId = 1, Title = "Sayfa", MetaTitle = "Meta" }]
        });
        var page = (await _sut.GetAllAsync()).Single();

        var updateResult = await _sut.UpdateAsync(page.Id, new UpdatePageRequest
        {
            Translations = [new PageTranslationInput { LanguageId = 1, Title = "Sayfa Güncel", MetaTitle = null }]
        });

        updateResult.Succeeded.Should().BeTrue();
        var updated = await _sut.GetByIdAsync(page.Id);
        updated!.Translations.Single(t => t.LanguageId == 1).Title.Should().Be("Sayfa Güncel");
        updated.Translations.Single(t => t.LanguageId == 1).MetaTitle.Should().BeNull();
    }

    [Fact]
    public async Task Page_CanHaveMultipleContentBlocks_OneToMany()
    {
        // Kritik doğrulama (Task 12): Page↔PageContentBlock bire-çok — bir Page birden fazla blok
        // içerebilir (önceki kapanış raporundaki "1:1" ifadesi yalnızca "tek-sahip" anlamındaydı).
        await _sut.CreateAsync(new CreatePageRequest { Translations = TrOnly("Çok Bloklu Sayfa") });
        var page = (await _sut.GetAllAsync()).Single();

        await _blockService.AddAsync(new AddPageContentBlockRequest
        {
            PageId = page.Id,
            BlockType = PageBlockType.Accordion,
            Translations = [new PageContentBlockTranslationInput { LanguageId = 1, Title = "Blok 1" }]
        });
        await _blockService.AddAsync(new AddPageContentBlockRequest
        {
            PageId = page.Id,
            BlockType = PageBlockType.Tab,
            Translations = [new PageContentBlockTranslationInput { LanguageId = 1, Title = "Blok 2" }]
        });

        var blocksInDb = await _ctx.DbContext.PageContentBlocks.Where(b => b.PageId == page.Id).ToListAsync();
        blocksInDb.Should().HaveCount(2);

        var distinctPageIds = blocksInDb.Select(b => b.PageId).Distinct().ToList();
        distinctPageIds.Should().ContainSingle().Which.Should().Be(page.Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAllBlocksTranslationsAndPhysicalFiles()
    {
        await _sut.CreateAsync(new CreatePageRequest { Translations = TrOnly("Silinecek Sayfa") });
        var page = (await _sut.GetAllAsync()).Single();

        var (fileName, contentType, length, content) = Factories.ImageUploadFactory.ValidJpeg();
        await using (content)
        {
            await _blockService.AddAsync(new AddPageContentBlockRequest
            {
                PageId = page.Id,
                BlockType = PageBlockType.FullWidthImage,
                Translations = [],
                ImageOriginalFileName = fileName,
                ImageContentType = contentType,
                ImageLength = length,
                ImageContent = content
            });
        }

        var block = (await _blockService.GetByPageIdAsync(page.Id)).Single();
        var imagePath = block.ImagePath!;

        var deleteResult = await _sut.DeleteAsync(page.Id);

        deleteResult.Succeeded.Should().BeTrue();
        (await _ctx.DbContext.PageContentBlocks.Where(b => b.PageId == page.Id).ToListAsync()).Should().BeEmpty();
        _ctx.Translations.HasAnyTranslationsFor(EntityType.Page, page.Id).Should().BeFalse();
        _ctx.Translations.HasAnyTranslationsFor(EntityType.PageContentBlock, block.Id).Should().BeFalse();
        _ctx.FileStorage.DeleteCalls.Should().Contain(imagePath);
    }
}
