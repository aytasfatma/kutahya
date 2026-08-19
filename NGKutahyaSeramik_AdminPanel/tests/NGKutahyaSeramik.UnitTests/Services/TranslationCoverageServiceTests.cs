using Application.Banners;
using Application.Blogs;
using Application.Categories;
using Application.Collections;
using Application.News;
using Application.Pages;
using Application.ProductImages;
using Application.Products;
using Application.ReferenceProjects;
using Application.Translations;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// ADR-007'nin ertelenen kısmı — eksik çeviri tespiti. "Sociable unit test": gerçek SQLite in-memory
/// DB + gerçek modül servisleri (ProductService/CategoryService/... — SeoManagementService'in zaten
/// kanıtladığı deseni izler), yalnızca ITranslationService/IFileStorageService bellek-içi sahte
/// (ServiceTestContext, FakeTranslationService varsayılan olarak TR=1/EN=2 aktif dil seed eder).
/// Kesin kural (görev talimatı): fallback/başka dilden doldurma YOK — testler bunu, bir dilde değer
/// varken diğerinde YOKSA ikisinin de bağımsız değerlendirildiğini doğrulayarak kanıtlar.
/// </summary>
public sealed class TranslationCoverageServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();
    private readonly TranslationCoverageService _sut;
    private readonly ProductRepository _productRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly CollectionRepository _collectionRepository;
    private readonly ProductService _productService;
    private readonly CategoryService _categoryService;
    private readonly BannerService _bannerService;
    private readonly BlogCategoryService _blogCategoryService;

    public TranslationCoverageServiceTests()
    {
        _productRepository = new ProductRepository(_ctx.DbContext);
        _categoryRepository = new CategoryRepository(_ctx.DbContext);
        _collectionRepository = new CollectionRepository(_ctx.DbContext);
        var productImageRepository = new ProductImageRepository(_ctx.DbContext);
        var blogRepository = new BlogRepository(_ctx.DbContext);
        var blogCategoryRepository = new BlogCategoryRepository(_ctx.DbContext);
        var tagRepository = new TagRepository(_ctx.DbContext);
        var newsRepository = new NewsRepository(_ctx.DbContext);
        var newsCategoryRepository = new NewsCategoryRepository(_ctx.DbContext);
        var pageRepository = new PageRepository(_ctx.DbContext);
        var pageContentBlockRepository = new PageContentBlockRepository(_ctx.DbContext);
        var bannerRepository = new BannerRepository(_ctx.DbContext);
        var referenceProjectRepository = new ReferenceProjectRepository(_ctx.DbContext);
        var referenceProjectImageRepository = new ReferenceProjectImageRepository(_ctx.DbContext);

        var productImageService = new ProductImageService(
            productImageRepository, _productRepository, _ctx.FileStorage, _ctx.UnitOfWork, NullLogger<ProductImageService>.Instance);
        var referenceProjectImageService = new ReferenceProjectImageService(
            referenceProjectImageRepository, referenceProjectRepository, _ctx.FileStorage, _ctx.UnitOfWork, NullLogger<ReferenceProjectImageService>.Instance);
        var pageContentBlockService = new PageContentBlockService(
            pageContentBlockRepository, pageRepository, _ctx.Translations, _ctx.FileStorage, _ctx.UnitOfWork, NullLogger<PageContentBlockService>.Instance);

        _productService = new ProductService(_productRepository, _categoryRepository, _collectionRepository, productImageRepository, productImageService, _ctx.Translations, _ctx.UnitOfWork);
        _categoryService = new CategoryService(_categoryRepository, _productRepository, _ctx.Translations, _ctx.UnitOfWork);
        var collectionService = new CollectionService(_collectionRepository, _productRepository, _ctx.Translations, _ctx.UnitOfWork);
        var blogService = new BlogService(blogRepository, blogCategoryRepository, tagRepository, _ctx.Translations, _ctx.FileStorage, _ctx.UnitOfWork, NullLogger<BlogService>.Instance);
        _blogCategoryService = new BlogCategoryService(blogCategoryRepository, _ctx.Translations, _ctx.UnitOfWork);
        var newsService = new NewsService(newsRepository, newsCategoryRepository, _ctx.Translations, _ctx.FileStorage, _ctx.UnitOfWork, NullLogger<NewsService>.Instance);
        var newsCategoryService = new NewsCategoryService(newsCategoryRepository, _ctx.Translations, _ctx.UnitOfWork);
        var pageService = new PageService(pageRepository, _ctx.Translations, pageContentBlockService, _ctx.UnitOfWork);
        _bannerService = new BannerService(bannerRepository, _ctx.Translations, _ctx.FileStorage, _ctx.UnitOfWork, NullLogger<BannerService>.Instance);
        var referenceProjectService = new ReferenceProjectService(referenceProjectRepository, _productRepository, referenceProjectImageRepository, referenceProjectImageService, _ctx.Translations, _ctx.UnitOfWork);

        _sut = new TranslationCoverageService(
            _productService, _categoryService, collectionService, blogService, _blogCategoryService,
            newsService, newsCategoryService, pageService, pageContentBlockService, _bannerService,
            referenceProjectService, _ctx.Translations);
    }

    public void Dispose() => _ctx.Dispose();

    private async Task<(int CategoryId, int CollectionId)> SeedCategoryAndCollectionAsync()
    {
        var category = CategoryFactory.CreateRoot();
        await _categoryRepository.AddAsync(category);
        await _ctx.UnitOfWork.SaveChangesAsync();

        var collection = CollectionFactory.CreateValid();
        await _collectionRepository.AddAsync(collection);
        await _ctx.UnitOfWork.SaveChangesAsync();

        return (category.Id, collection.Id);
    }

    [Fact]
    public async Task GetReportAsync_WithNoData_ReturnsZeroTotal_ButListsAllActiveLanguagesAndSupportedTypes()
    {
        var report = await _sut.GetReportAsync();

        report.TotalMissing.Should().Be(0);
        report.ByLanguage.Should().HaveCount(2, "FakeTranslationService varsayılanı TR+EN");
        report.ByModule.Should().HaveCount(9, "kategori ve koleksiyon artık çeviri altyapısı kullanmıyor");
        report.ByModule.Select(m => m.EntityType).Should().NotContain(EntityType.Dealer, "Dealer'ın hiç çeviri alanı yok (ADR-008)");
    }

    [Fact]
    public async Task GetReportAsync_ProductWithOnlyTrTranslation_FlagsAllEnglishFieldsAsMissing()
    {
        var (categoryId, collectionId) = await SeedCategoryAndCollectionAsync();
        var createResult = await _productService.CreateAsync(new CreateProductRequest
        {
            ProductCode = "COV0001RP",
            CategoryId = categoryId,
            CollectionId = collectionId,
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
            Translations = [new ProductTranslationInput { LanguageId = 1, Name = "Amazonit", ShortDescription = "Kısa açıklama" }]
        });
        createResult.Succeeded.Should().BeTrue(createResult.ErrorMessage);

        var report = await _sut.GetReportAsync();

        var product = (await _productService.GetAllAsync()).Single();
        var enMissing = report.Items.Where(i => i.EntityType == EntityType.Product && i.EntityId == product.Id && i.LanguageCode == "EN").ToList();

        enMissing.Select(i => i.FieldName).Should().BeEquivalentTo(
            new[] { ProductFields.Name, ProductFields.ShortDescription, ProductFields.LongDescription, ProductFields.SeoUrl, ProductFields.MetaTitle, ProductFields.MetaDescription },
            "TR'de değer olsun olmasın, EN hiç girilmediği için TÜM Ürün alanları EN için eksik");

        var trMissing = report.Items.Where(i => i.EntityType == EntityType.Product && i.EntityId == product.Id && i.LanguageCode == "TR").ToList();
        trMissing.Select(i => i.FieldName).Should().BeEquivalentTo(
            new[] { ProductFields.LongDescription, ProductFields.SeoUrl, ProductFields.MetaTitle, ProductFields.MetaDescription },
            "TR'de yalnızca Name/ShortDescription girildi — geri kalanı TR için de eksik");
    }

    [Fact]
    public async Task GetReportAsync_NoFallback_MissingLanguageStaysMissing_EvenWhenOtherLanguageHasValue()
    {
        // Kesin kural (görev talimatı): "Eksik çeviriyi başka dilden doldurma" — TR'de değer olması
        // EN'nin eksik sayılmasını hiçbir şekilde etkilemez.
        var (categoryId, collectionId) = await SeedCategoryAndCollectionAsync();
        var createResult = await _productService.CreateAsync(new CreateProductRequest
        {
            ProductCode = "COV0002RP",
            CategoryId = categoryId,
            CollectionId = collectionId,
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
                new ProductTranslationInput { LanguageId = 1, Name = "Amazonit", ShortDescription = "Kısa açıklama" },
                new ProductTranslationInput { LanguageId = 2, Name = "Amazonite" }
            ]
        });
        createResult.Succeeded.Should().BeTrue(createResult.ErrorMessage);

        var report = await _sut.GetReportAsync();
        var product = (await _productService.GetAllAsync()).Single();

        report.Items.Should().NotContain(i =>
            i.EntityType == EntityType.Product && i.EntityId == product.Id && i.FieldName == ProductFields.Name,
            "hem TR hem EN Name dolu — eksik değil");
        report.Items.Should().Contain(i =>
            i.EntityType == EntityType.Product && i.EntityId == product.Id && i.LanguageCode == "EN" && i.FieldName == ProductFields.ShortDescription,
            "EN ShortDescription hiç girilmedi — TR'de de yok, fallback uygulanmadığı için eksik kalmalı");
    }

    [Fact]
    public async Task GetReportAsync_CategoryWithFullTranslations_ProducesNoMissingEntriesForThatRecord()
    {
        var createResult = await _categoryService.CreateAsync(new CreateCategoryRequest
        {
            DisplayOrder = 1,
            Translations =
            [
                new CategoryTranslationInput { LanguageId = 1, Name = "Kategori Adı", Description = "Açıklama", SeoUrl = "kategori-adi", MetaTitle = "Meta Başlık", MetaDescription = "Meta Açıklama" },
                new CategoryTranslationInput { LanguageId = 2, Name = "Category Name", Description = "Description", SeoUrl = "category-name", MetaTitle = "Meta Title", MetaDescription = "Meta Description" }
            ]
        });
        createResult.Succeeded.Should().BeTrue();
        var category = (await _categoryService.GetTreeAsync()).Single();

        var report = await _sut.GetReportAsync();

        report.Items.Should().NotContain(i => i.EntityType == EntityType.Category && i.EntityId == category.Id);
    }

    [Fact]
    public async Task GetReportAsync_BlogCategory_SingleNameField_DetectedIndependentlyOfBlog()
    {
        var blogCategory = new Domain.Entities.BlogCategory(0);
        await _ctx.DbContext.BlogCategories.AddAsync(blogCategory);
        await _ctx.UnitOfWork.SaveChangesAsync();

        var report = await _sut.GetReportAsync();

        report.Items.Where(i => i.EntityType == EntityType.BlogCategory && i.EntityId == blogCategory.Id)
            .Select(i => i.FieldName).Should().BeEquivalentTo([BlogCategoryFields.Name, BlogCategoryFields.Name], "TR+EN için Name eksik");
        report.ByModule.Single(m => m.EntityType == EntityType.BlogCategory).MissingCount.Should().Be(2);
    }

    [Fact]
    public async Task GetReportAsync_Banner_FourFieldsPerLanguage_NoSeoFieldsInvented()
    {
        var bannerResult = await _bannerService.CreateAsync(new CreateBannerRequest
        {
            DisplayOrder = 1,
            Translations = [new BannerTranslationInput { LanguageId = 1, Title = "Kampanya" }]
        });
        bannerResult.Succeeded.Should().BeTrue();

        var report = await _sut.GetReportAsync();
        var banner = (await _bannerService.GetAllAsync()).Single();

        var trFields = report.Items.Where(i => i.EntityType == EntityType.Banner && i.EntityId == banner.Id && i.LanguageCode == "TR").Select(i => i.FieldName).ToList();
        trFields.Should().BeEquivalentTo(BannerFields.Subtitle, BannerFields.ButtonText, BannerFields.ButtonUrl);
        trFields.Should().NotContain("SeoUrl", "Banner'ın hiç SEO alanı yok (BannerFields.cs) — icat edilmedi");
    }
}
