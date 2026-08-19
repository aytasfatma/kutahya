using Application.ProductImages;
using Application.Products;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.UnitTests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly ProductService _sut;
    private readonly CategoryRepository _categoryRepository;
    private readonly CollectionRepository _collectionRepository;
    private readonly int _categoryId;
    private readonly int _collectionId;

    public ProductServiceTests()
    {
        var productRepository = new ProductRepository(_ctx.DbContext);
        _categoryRepository = new CategoryRepository(_ctx.DbContext);
        _collectionRepository = new CollectionRepository(_ctx.DbContext);
        var productImageRepository = new ProductImageRepository(_ctx.DbContext);

        var productImageService = new ProductImageService(
            productImageRepository, productRepository, _ctx.FileStorage,
            _ctx.UnitOfWork, NullLogger<ProductImageService>.Instance);

        _sut = new ProductService(
            productRepository, _categoryRepository, _collectionRepository, productImageRepository,
            productImageService, _ctx.Translations, _ctx.UnitOfWork);

        var category = CategoryFactory.CreateRoot();
        _categoryRepository.AddAsync(category).GetAwaiter().GetResult();
        _ctx.UnitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
        _categoryId = category.Id;

        var collection = CollectionFactory.CreateValid();
        _collectionRepository.AddAsync(collection).GetAwaiter().GetResult();
        _ctx.UnitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
        _collectionId = collection.Id;
    }

    public void Dispose() => _ctx.Dispose();

    private CreateProductRequest ValidRequest(string productCode = "TEST0001RP", bool withoutCategory = false) => new()
    {
        ProductCode = productCode,
        CategoryId = withoutCategory ? null : _categoryId,
        CollectionId = _collectionId,
        Brand = ProductBrand.NgSeramik,
        Status = ProductStatus.Active,
        Size = "60x120",
        Unit = "M2",
        Surface = "MAT",
        Thickness = 9.0m,
        BodyType = "SIRLI PORSELEN",
        Color = "Beyaz",
        ApplicationArea = "YER",
        UsageArea = "BANYO",
        DisplayOrder = 1,
        Translations =
        [
            new ProductTranslationInput { LanguageId = 1, Name = "Amazonit", ShortDescription = "Kısa açıklama" }
        ]
    };

    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        var result = await _sut.CreateAsync(ValidRequest());

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().ContainSingle(p => p.ProductCode == "TEST0001RP");
    }

    [Fact]
    public async Task CreateAsync_WithoutCategory_Succeeds()
    {
        var request = ValidRequest("NOCAT001RP", withoutCategory: true);

        var result = await _sut.CreateAsync(request);

        result.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Single(p => p.ProductCode == "NOCAT001RP").CategoryId.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_PersistsTranslationFields()
    {
        await _sut.CreateAsync(ValidRequest());

        var product = (await _sut.GetAllAsync()).Single();
        var tr = product.Translations.Single(t => t.LanguageId == 1);

        tr.Name.Should().Be("Amazonit");
        tr.ShortDescription.Should().Be("Kısa açıklama");
    }

    [Fact]
    public async Task CreateAsync_DuplicateProductCode_IsRejected()
    {
        await _sut.CreateAsync(ValidRequest("DUP0001RP"));

        var result = await _sut.CreateAsync(ValidRequest("DUP0001RP"));

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ürün kodu");
    }

    [Fact]
    public async Task CreateAsync_DuplicateProductCode_CaseInsensitiveTrim_IsRejected()
    {
        await _sut.CreateAsync(ValidRequest("CASE001RP"));

        var request = ValidRequest("  case001rp  ");
        var result = await _sut.CreateAsync(request);

        // Not: Trim uygulanır; case-sensitivity DB collation'a bağlıdır (SQL_Latin1_General_CP1_CI_AS
        // — case-insensitive), bu yüzden gerçek ortamda da bu senaryo reddedilir.
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentCategory_IsRejected()
    {
        var invalidRequest = new CreateProductRequest
        {
            ProductCode = "NOCAT001RP",
            CategoryId = 999_999,
            CollectionId = _collectionId,
            Brand = ProductBrand.NgSeramik,
            Status = ProductStatus.Active,
            Size = "60x120",
            Unit = "M2",
            Surface = "MAT",
            Thickness = 9.0m,
            BodyType = "SIRLI PORSELEN",
            Color = "Beyaz",
            ApplicationArea = "YER",
            UsageArea = "BANYO",
            Translations = [new ProductTranslationInput { LanguageId = 1, Name = "X", ShortDescription = "Y" }]
        };

        var result = await _sut.CreateAsync(invalidRequest);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("kategori");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentCollection_IsRejected()
    {
        var invalidRequest = new CreateProductRequest
        {
            ProductCode = "NOCOL001RP",
            CategoryId = _categoryId,
            CollectionId = 999_999,
            Brand = ProductBrand.NgSeramik,
            Status = ProductStatus.Active,
            Size = "60x120",
            Unit = "M2",
            Surface = "MAT",
            Thickness = 9.0m,
            BodyType = "SIRLI PORSELEN",
            Color = "Beyaz",
            ApplicationArea = "YER",
            UsageArea = "BANYO",
            Translations = [new ProductTranslationInput { LanguageId = 1, Name = "X", ShortDescription = "Y" }]
        };

        var result = await _sut.CreateAsync(invalidRequest);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("koleksiyon");
    }

    [Fact]
    public async Task CreateAsync_WithZeroThickness_IsRejected()
    {
        var request = ValidRequest("THICK001RP");
        var invalid = new CreateProductRequest
        {
            ProductCode = request.ProductCode,
            CategoryId = request.CategoryId,
            CollectionId = request.CollectionId,
            Brand = request.Brand,
            Status = request.Status,
            Size = request.Size,
            Unit = request.Unit,
            Surface = request.Surface,
            Thickness = 0,
            BodyType = request.BodyType,
            Color = request.Color,
            ApplicationArea = request.ApplicationArea,
            UsageArea = request.UsageArea,
            Translations = request.Translations
        };

        var result = await _sut.CreateAsync(invalid);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyWithCategoryIdAsync_ReflectsExistingProduct_SupportsDeleteGuard()
    {
        await _sut.CreateAsync(ValidRequest("GUARD001RP"));

        var productRepository = new ProductRepository(_ctx.DbContext);
        var hasAny = await productRepository.HasAnyWithCategoryIdAsync(_categoryId);

        hasAny.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesProductAndTranslations()
    {
        await _sut.CreateAsync(ValidRequest("DEL0001RP"));
        var product = (await _sut.GetAllAsync()).Single();

        var deleteResult = await _sut.DeleteAsync(product.Id);

        deleteResult.Succeeded.Should().BeTrue();
        (await _sut.GetAllAsync()).Should().BeEmpty();
        _ctx.Translations.HasAnyTranslationsFor(Domain.Enums.EntityType.Product, product.Id).Should().BeFalse();
    }
}
