using Application.Dealers;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// Madde 25 — Bayi Yönetimi. DealerService projedeki ilk servis: Translation ve IFileStorageService
/// hiç kullanılmıyor (bkz. Dealer.cs XML doc), bu yüzden ServiceTestContext'in yalnızca DbContext'i
/// ve UnitOfWork'ü kullanılıyor.
/// </summary>
public class DealerServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly DealerService _sut;

    public DealerServiceTests()
    {
        _sut = new DealerService(new DealerRepository(_ctx.DbContext), _ctx.UnitOfWork);
    }

    public void Dispose() => _ctx.Dispose();

    private static CreateDealerRequest ValidDealerRequest() => new()
    {
        Name = "Test Bayi",
        City = "İstanbul",
        Category = DealerCategory.SalesPoint,
        District = "Kadıköy",
        Address = "Test Adres",
        Phone = "05551234567",
        Email = "test@example.com",
        Latitude = 40.990000m,
        Longitude = 29.030000m,
        Region = "SADN",
        RegionName = "Anadolu Yakası"
    };

    [Fact]
    public async Task CreateAsync_WithValidDealer_Succeeds()
    {
        var result = await _sut.CreateAsync(ValidDealerRequest());

        result.Succeeded.Should().BeTrue();
        var dealer = (await _sut.GetAllAsync()).Single();
        dealer.Category.Should().Be(DealerCategory.SalesPoint);
        dealer.Name.Should().Be("Test Bayi");
    }

    [Fact]
    public async Task CreateAsync_WithValidShowroom_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateDealerRequest
        {
            Name = "Kütahya Merkez Showroom",
            City = "Kütahya",
            Category = DealerCategory.Factory
        });

        result.Succeeded.Should().BeTrue();
        var dealer = (await _sut.GetAllAsync()).Single();
        dealer.Category.Should().Be(DealerCategory.Factory);
        dealer.CategoryLabel.Should().Be("Fabrika");
    }

    [Fact]
    public async Task CreateAsync_WithoutCategory_Succeeds_RepresentsUnclassifiedRecord()
    {
        // Madde 25 Ek-2: 212 kayıttan 17'si kategorisiz — gerçek veri senaryosu.
        var result = await _sut.CreateAsync(new CreateDealerRequest
        {
            Name = "Kategorisiz Kayıt",
            City = "Bursa",
            Category = null
        });

        result.Succeeded.Should().BeTrue();
        var dealer = (await _sut.GetAllAsync()).Single();
        dealer.Category.Should().BeNull();
        dealer.CategoryLabel.Should().Be("Kategorisiz");
    }

    [Fact]
    public async Task CreateAsync_WithoutName_IsRejected()
    {
        var request = ValidDealerRequest();
        var invalid = new CreateDealerRequest
        {
            Name = "   ",
            City = request.City,
            Category = request.Category
        };

        var result = await _sut.CreateAsync(invalid);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("adı");
    }

    [Fact]
    public async Task CreateAsync_WithoutCity_IsRejected()
    {
        var request = new CreateDealerRequest { Name = "Test Bayi", City = "" };

        var result = await _sut.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Şehir");
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public async Task CreateAsync_WithOutOfRangeLatitude_IsRejected(decimal invalidLatitude)
    {
        var request = new CreateDealerRequest
        {
            Name = "Test Bayi",
            City = "İstanbul",
            Latitude = invalidLatitude,
            Longitude = 29.0m
        };

        var result = await _sut.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Enlem");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public async Task CreateAsync_WithOutOfRangeLongitude_IsRejected(decimal invalidLongitude)
    {
        var request = new CreateDealerRequest
        {
            Name = "Test Bayi",
            City = "İstanbul",
            Latitude = 40.0m,
            Longitude = invalidLongitude
        };

        var result = await _sut.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Boylam");
    }

    [Fact]
    public async Task CreateAsync_WithOnlyLatitude_IsRejected()
    {
        var request = new CreateDealerRequest
        {
            Name = "Test Bayi",
            City = "İstanbul",
            Latitude = 40.99m,
            Longitude = null
        };

        var result = await _sut.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("birlikte");
    }

    [Fact]
    public async Task CreateAsync_WithOnlyLongitude_IsRejected()
    {
        var request = new CreateDealerRequest
        {
            Name = "Test Bayi",
            City = "İstanbul",
            Latitude = null,
            Longitude = 29.03m
        };

        var result = await _sut.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("birlikte");
    }

    [Fact]
    public async Task CreateAsync_WithoutCoordinates_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateDealerRequest { Name = "Test Bayi", City = "İstanbul" });

        result.Succeeded.Should().BeTrue();
        var dealer = (await _sut.GetAllAsync()).Single();
        dealer.Latitude.Should().BeNull();
        dealer.Longitude.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ChangesFieldsAndClearsOptionalOnes()
    {
        await _sut.CreateAsync(ValidDealerRequest());
        var created = (await _sut.GetAllAsync()).Single();

        var updateResult = await _sut.UpdateAsync(created.Id, new UpdateDealerRequest
        {
            Name = "Güncellenmiş Bayi",
            City = "Ankara",
            Category = DealerCategory.GeneralHeadquarters,
            District = null,
            Phone = null
        });

        updateResult.Succeeded.Should().BeTrue();
        var updated = (await _sut.GetAllAsync()).Single();
        updated.Name.Should().Be("Güncellenmiş Bayi");
        updated.City.Should().Be("Ankara");
        updated.Category.Should().Be(DealerCategory.GeneralHeadquarters);
        updated.District.Should().BeNull();
        updated.Phone.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentDealer_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync(999, new UpdateDealerRequest { Name = "X", City = "Y" });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task ToggleActiveAsync_FlipsIsActive()
    {
        await _sut.CreateAsync(ValidDealerRequest());
        var dealer = (await _sut.GetAllAsync()).Single();
        dealer.IsActive.Should().BeTrue();

        await _sut.ToggleActiveAsync(dealer.Id);
        (await _sut.GetAllAsync()).Single().IsActive.Should().BeFalse();

        await _sut.ToggleActiveAsync(dealer.Id);
        (await _sut.GetAllAsync()).Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesDealer()
    {
        await _sut.CreateAsync(ValidDealerRequest());
        var dealer = (await _sut.GetAllAsync()).Single();

        var result = await _sut.DeleteAsync(dealer.Id);

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentDealer_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync(999);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }
}
