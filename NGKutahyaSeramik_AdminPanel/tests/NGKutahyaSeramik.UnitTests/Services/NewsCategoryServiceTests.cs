using Application.News;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.UnitTests.Services;

public class NewsCategoryServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly NewsCategoryService _sut;

    public NewsCategoryServiceTests()
    {
        _sut = new NewsCategoryService(
            new NewsCategoryRepository(_ctx.DbContext),
            _ctx.Translations,
            _ctx.UnitOfWork);
    }

    public void Dispose() => _ctx.Dispose();

    private static IReadOnlyList<NewsCategoryTranslationInput> TrOnly(string name) =>
    [
        new NewsCategoryTranslationInput { LanguageId = 1, Name = name }
    ];

    [Fact]
    public async Task CreateAsync_WithValidTrName_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateNewsCategoryRequest
        {
            DisplayOrder = 1,
            Translations = TrOnly("Ödüller")
        });

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().ContainSingle(c => c.Translations.Any(t => t.Name == "Ödüller"));
    }

    [Fact]
    public async Task CreateAsync_WithoutTrName_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateNewsCategoryRequest
        {
            DisplayOrder = 1,
            Translations = [new NewsCategoryTranslationInput { LanguageId = 2, Name = "English Only" }]
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Türkçe");
    }

    [Fact]
    public async Task CreateAsync_WithNegativeDisplayOrder_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateNewsCategoryRequest
        {
            DisplayOrder = -1,
            Translations = TrOnly("Kutlamalar")
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("1 veya daha büyük");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateTrName_IsRejected()
    {
        await _sut.CreateAsync(new CreateNewsCategoryRequest { DisplayOrder = 1, Translations = TrOnly("Sertifikalar") });

        var result = await _sut.CreateAsync(new CreateNewsCategoryRequest { DisplayOrder = 1, Translations = TrOnly("sertifikalar") });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("zaten var");
    }

    [Fact]
    public async Task UpdateAsync_KeepingSameName_DoesNotTriggerDuplicateRejection()
    {
        await _sut.CreateAsync(new CreateNewsCategoryRequest { DisplayOrder = 1, Translations = TrOnly("Bültenler") });
        var category = (await _sut.GetAllAsync()).Single();

        var result = await _sut.UpdateAsync(category.Id, new UpdateNewsCategoryRequest
        {
            DisplayOrder = 5,
            Translations = TrOnly("Bültenler")
        });

        result.Succeeded.Should().BeTrue();
        var updated = (await _sut.GetAllAsync()).Single();
        updated.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task ToggleActiveAsync_FlipsIsActive()
    {
        await _sut.CreateAsync(new CreateNewsCategoryRequest { DisplayOrder = 1, Translations = TrOnly("Kurumsal") });
        var category = (await _sut.GetAllAsync()).Single();
        category.IsActive.Should().BeTrue();

        await _sut.ToggleActiveAsync(category.Id);
        (await _sut.GetAllAsync()).Single().IsActive.Should().BeFalse();

        await _sut.ToggleActiveAsync(category.Id);
        (await _sut.GetAllAsync()).Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategoryAndTranslations()
    {
        await _sut.CreateAsync(new CreateNewsCategoryRequest { DisplayOrder = 1, Translations = TrOnly("Silinecek Kategori") });
        var category = (await _sut.GetAllAsync()).Single();

        var result = await _sut.DeleteAsync(category.Id);

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().BeEmpty();
        _ctx.Translations.HasAnyTranslationsFor(EntityType.NewsCategory, category.Id).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentCategory_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync(999);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }
}
