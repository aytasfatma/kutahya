using Application.Languages;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// Backlog #3 — Dil Yönetimi panel modülü. LanguageService, Translation/dosya kullanmıyor (Dealer
/// deseniyle aynı) — yalnızca ServiceTestContext'in DbContext/UnitOfWork'ü kullanılıyor.
/// </summary>
public class LanguageServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly LanguageService _sut;

    public LanguageServiceTests()
    {
        _sut = new LanguageService(new LanguageRepository(_ctx.DbContext), _ctx.UnitOfWork);
    }

    public void Dispose() => _ctx.Dispose();

    private async Task<int> SeedTurkishAsync()
    {
        var tr = LanguageFactory.CreateTurkish();
        _ctx.DbContext.Languages.Add(tr);
        await _ctx.DbContext.SaveChangesAsync();
        return tr.Id;
    }

    private async Task<int> SeedEnglishAsync()
    {
        var en = LanguageFactory.CreateEnglish();
        _ctx.DbContext.Languages.Add(en);
        await _ctx.DbContext.SaveChangesAsync();
        return en.Id;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsLanguagesOrderedByDisplayOrder()
    {
        await SeedEnglishAsync();
        await SeedTurkishAsync();

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Code.Should().Be("TR", "TR DisplayOrder=1, EN DisplayOrder=2");
        result[1].Code.Should().Be("EN");
    }

    [Fact]
    public async Task UpdateAsync_ValidChange_UpdatesNameAndDisplayOrder()
    {
        var id = await SeedEnglishAsync();

        var result = await _sut.UpdateAsync(id, new UpdateLanguageRequest { Name = "İngilizce", DisplayOrder = 5, IsActive = true });

        result.Succeeded.Should().BeTrue();
        var updated = await _sut.GetByIdAsync(id);
        updated!.Name.Should().Be("İngilizce");
        updated.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_DeactivateNonTurkishLanguage_Succeeds()
    {
        var id = await SeedEnglishAsync();

        var result = await _sut.UpdateAsync(id, new UpdateLanguageRequest { Name = "English", DisplayOrder = 2, IsActive = false });

        result.Succeeded.Should().BeTrue();
        (await _sut.GetByIdAsync(id))!.IsActive.Should().BeFalse();
    }

    // Kesin kural (görev talimatı): "Türkçe devre dışı bırakılamaz".
    [Fact]
    public async Task UpdateAsync_DeactivateTurkish_IsRejected()
    {
        var id = await SeedTurkishAsync();

        var result = await _sut.UpdateAsync(id, new UpdateLanguageRequest { Name = "Türkçe", DisplayOrder = 1, IsActive = false });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Türkçe");
        (await _sut.GetByIdAsync(id))!.IsActive.Should().BeTrue("reddedilen istek DB'yi değiştirmemeli");
    }

    // Code, request modelinde hiç yok — bu test Code'un asla değişmeyeceğini kanıtlıyor (Name/DisplayOrder
    // değişse bile). Ayrıca TR'nin IsActive=true kalarak Name/DisplayOrder güncellenebildiğini doğrular.
    [Fact]
    public async Task UpdateAsync_TurkishKeepsActive_NameAndDisplayOrderStillUpdatable()
    {
        var id = await SeedTurkishAsync();

        var result = await _sut.UpdateAsync(id, new UpdateLanguageRequest { Name = "Türkçe (Güncel)", DisplayOrder = 3, IsActive = true });

        result.Succeeded.Should().BeTrue();
        var updated = await _sut.GetByIdAsync(id);
        updated!.Code.Should().Be("TR");
        updated.Name.Should().Be("Türkçe (Güncel)");
        updated.DisplayOrder.Should().Be(3);
        updated.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_BlankName_IsRejected()
    {
        var id = await SeedEnglishAsync();

        var result = await _sut.UpdateAsync(id, new UpdateLanguageRequest { Name = "   ", DisplayOrder = 2, IsActive = true });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("zorunlu");
    }

    [Fact]
    public async Task UpdateAsync_NegativeDisplayOrder_IsRejected()
    {
        var id = await SeedEnglishAsync();

        var result = await _sut.UpdateAsync(id, new UpdateLanguageRequest { Name = "English", DisplayOrder = -1, IsActive = true });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("1 veya daha büyük");
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync(999, new UpdateLanguageRequest { Name = "X", DisplayOrder = 0, IsActive = true });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        (await _sut.GetByIdAsync(999)).Should().BeNull();
    }
}
