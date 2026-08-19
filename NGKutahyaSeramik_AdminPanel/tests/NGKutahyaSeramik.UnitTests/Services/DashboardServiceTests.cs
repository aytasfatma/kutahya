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
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;
using NGKutahyaSeramik.UnitTests.Mocks;
using Xunit;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// Madde 17.2 modül #1 (Dashboard, Task 18). `SqliteTestDatabase` (gerçek SQLite in-memory,
/// `UseInMemoryDatabase` DEĞİL) kullanılıyor — `DashboardService` doğrudan `AppDbContext`'e bağımlı
/// olduğu için (bkz. IDashboardService xmldoc) burada bir repository/UnitOfWork sahtesine gerek yok,
/// entity'ler doğrudan DbContext'e eklenip `SaveChangesAsync` ile kalıcılaştırılıyor.
/// `CreatedAt` alanları private-setter olduğu için sıralama testlerinde EF Core'un
/// `Entry(x).Property(...).CurrentValue` API'siyle deterministik zaman damgaları veriliyor —
/// `DateTime.UtcNow`'un art arda çağrılarına (aynı tick riski) güvenilmiyor.
///
/// `CreateSut` — Task 18 sonrası DashboardService, MissingTranslationCount için TranslationCoverageService'e
/// bağımlı hale geldi (bkz. DashboardService xmldoc); bu, TranslationCoverageService'in enjekte ettiği
/// 11 modül servisinin de burada gerçek repository'lerle (aynı `context`e bağlı) kurulmasını gerektiriyor
/// — hiçbiri bu testlerde ayrıca seed edilmediği için hepsi boş liste döner, MissingTranslationCount
/// hiçbir testte doğrudan assert edilmiyor (bkz. TranslationCoverageServiceTests.cs, ayrı ve odaklı).
/// </summary>
public class DashboardServiceTests
{
    private static async Task<(int CategoryId, int CollectionId)> SeedCategoryAndCollectionAsync(AppDbContext context)
    {
        var category = CategoryFactory.CreateRoot();
        var collection = CollectionFactory.CreateValid();
        context.Categories.Add(category);
        context.Collections.Add(collection);
        await context.SaveChangesAsync();
        return (category.Id, collection.Id);
    }

    private static void SetCreatedAt<T>(AppDbContext context, T entity, DateTime createdAt) where T : class
    {
        context.Entry(entity).Property("CreatedAt").CurrentValue = createdAt;
    }

    private static DashboardService CreateSut(AppDbContext context)
    {
        var unitOfWork = new UnitOfWork(context);
        var translationService = new TranslationService(context);
        var fileStorage = new FakeFileStorageService();

        var productRepository = new ProductRepository(context);
        var categoryRepository = new CategoryRepository(context);
        var collectionRepository = new CollectionRepository(context);
        var productImageRepository = new ProductImageRepository(context);
        var blogRepository = new BlogRepository(context);
        var blogCategoryRepository = new BlogCategoryRepository(context);
        var tagRepository = new TagRepository(context);
        var newsRepository = new NewsRepository(context);
        var newsCategoryRepository = new NewsCategoryRepository(context);
        var pageRepository = new PageRepository(context);
        var pageContentBlockRepository = new PageContentBlockRepository(context);
        var bannerRepository = new BannerRepository(context);
        var referenceProjectRepository = new ReferenceProjectRepository(context);
        var referenceProjectImageRepository = new ReferenceProjectImageRepository(context);

        var productImageService = new ProductImageService(
            productImageRepository, productRepository, fileStorage, unitOfWork, NullLogger<ProductImageService>.Instance);
        var referenceProjectImageService = new ReferenceProjectImageService(
            referenceProjectImageRepository, referenceProjectRepository, fileStorage, unitOfWork, NullLogger<ReferenceProjectImageService>.Instance);
        var pageContentBlockService = new PageContentBlockService(
            pageContentBlockRepository, pageRepository, translationService, fileStorage, unitOfWork, NullLogger<PageContentBlockService>.Instance);

        return new DashboardService(context, new MemoryCache(new MemoryCacheOptions()));
    }

    // 1. Doğru ürün sayısı
    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectTotalProducts()
    {
        using var db = SqliteTestDatabase.Create();
        var (categoryId, collectionId) = await SeedCategoryAndCollectionAsync(db.Context);
        db.Context.Products.Add(ProductFactory.CreateValid("P0001RP", categoryId, collectionId, displayOrder: 1));
        db.Context.Products.Add(ProductFactory.CreateValid("P0002RP", categoryId, collectionId, displayOrder: 2));
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalProducts.Should().Be(2);
    }

    // 2. Aktif ürün filtresi
    [Fact]
    public async Task GetDashboardAsync_ActiveProducts_FiltersOnStatusActive()
    {
        using var db = SqliteTestDatabase.Create();
        var (categoryId, collectionId) = await SeedCategoryAndCollectionAsync(db.Context);
        db.Context.Products.Add(ProductFactory.CreateValid("ACT0001RP", categoryId, collectionId, status: ProductStatus.Active, displayOrder: 1));
        db.Context.Products.Add(ProductFactory.CreateValid("INA0001RP", categoryId, collectionId, status: ProductStatus.Inactive, displayOrder: 2));
        db.Context.Products.Add(ProductFactory.CreateValid("CAN0001RP", categoryId, collectionId, status: ProductStatus.Cancelled, displayOrder: 3));
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalProducts.Should().Be(3);
        result.ActiveProducts.Should().Be(1);
    }

    // 3. Kategori sayısı
    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectTotalCategories()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.Categories.Add(CategoryFactory.CreateRoot(displayOrder: 1));
        db.Context.Categories.Add(CategoryFactory.CreateRoot(displayOrder: 2));
        db.Context.Categories.Add(CategoryFactory.CreateRoot(displayOrder: 3));
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalCategories.Should().Be(3);
    }

    // 4. Koleksiyon sayısı
    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectTotalCollections()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.Collections.Add(CollectionFactory.CreateValid());
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalCollections.Should().Be(1);
    }

    // 5. Bayi ve showroom sayıları Category ayrımına göre doğru döner
    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectDealerAndShowroomCounts()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.Dealers.Add(DealerFactory.CreateDealer());
        db.Context.Dealers.Add(DealerFactory.CreateDealer("Test Bayi 2"));
        db.Context.Dealers.Add(DealerFactory.CreateShowroom());
        db.Context.Dealers.Add(DealerFactory.CreateUnclassified());
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.DealerCount.Should().Be(2);
        result.ShowroomCount.Should().Be(1);
    }

    // 6. Toplam kullanıcı sayısı
    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectTotalUsers()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.Users.Add(ApplicationUserFactory.CreateAdmin());
        db.Context.Users.Add(ApplicationUserFactory.CreateContentEditor());
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalUsers.Should().Be(2);
    }

    // 7. Aktif kullanıcı sayısı IsActive'e göre
    [Fact]
    public async Task GetDashboardAsync_ActiveUsers_FiltersOnIsActive()
    {
        using var db = SqliteTestDatabase.Create();
        var active = ApplicationUserFactory.CreateAdmin();
        var inactive = ApplicationUserFactory.CreateContentEditor();
        inactive.IsActive = false;
        db.Context.Users.Add(active);
        db.Context.Users.Add(inactive);
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalUsers.Should().Be(2);
        result.ActiveUsers.Should().Be(1);
    }

    // 8. Form sayısı
    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectTotalFormSubmissions()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.FormSubmissions.Add(FormSubmissionFactory.CreateContact());
        db.Context.FormSubmissions.Add(FormSubmissionFactory.CreateSampleRequest());
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalFormSubmissions.Should().Be(2);
    }

    // 9. Okunmamış/bekleyen form filtreleri (iki bağımsız alan) doğru çalışır
    [Fact]
    public async Task GetDashboardAsync_UnreadAndUnprocessedForms_AreIndependentFilters()
    {
        using var db = SqliteTestDatabase.Create();
        var readAndProcessed = FormSubmissionFactory.CreateContact("Okunmuş İşlenmiş");
        readAndProcessed.MarkAsRead();
        readAndProcessed.MarkAsProcessed();

        var readButUnprocessed = FormSubmissionFactory.CreateContact("Okunmuş İşlenmemiş");
        readButUnprocessed.MarkAsRead();

        var unread = FormSubmissionFactory.CreateContact("Okunmamış");

        db.Context.FormSubmissions.AddRange(readAndProcessed, readButUnprocessed, unread);
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalFormSubmissions.Should().Be(3);
        result.UnreadFormSubmissions.Should().Be(1); // yalnızca "unread"
        result.UnprocessedFormSubmissions.Should().Be(2); // "unread" + "readButUnprocessed"
    }

    // 10. Son ürünler en yeni kayıttan eskiye sıralanır
    [Fact]
    public async Task GetDashboardAsync_RecentProducts_OrderedNewestFirst()
    {
        using var db = SqliteTestDatabase.Create();
        var (categoryId, collectionId) = await SeedCategoryAndCollectionAsync(db.Context);
        var oldest = ProductFactory.CreateValid("OLD0001RP", categoryId, collectionId, displayOrder: 1);
        var middle = ProductFactory.CreateValid("MID0001RP", categoryId, collectionId, displayOrder: 2);
        var newest = ProductFactory.CreateValid("NEW0001RP", categoryId, collectionId, displayOrder: 3);
        db.Context.Products.AddRange(oldest, middle, newest);
        await db.Context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        SetCreatedAt(db.Context, oldest, now.AddDays(-2));
        SetCreatedAt(db.Context, middle, now.AddDays(-1));
        SetCreatedAt(db.Context, newest, now);
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.RecentProducts.Select(p => p.ProductCode).Should().ContainInOrder("NEW0001RP", "MID0001RP", "OLD0001RP");
    }

    // 11. Son ürünler limit (5) kadar döner
    [Fact]
    public async Task GetDashboardAsync_RecentProducts_LimitedToFive()
    {
        using var db = SqliteTestDatabase.Create();
        var (categoryId, collectionId) = await SeedCategoryAndCollectionAsync(db.Context);
        for (var i = 0; i < 8; i++)
        {
            db.Context.Products.Add(ProductFactory.CreateValid($"LIM{i:D4}RP", categoryId, collectionId, displayOrder: i + 1));
        }
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalProducts.Should().Be(8);
        result.RecentProducts.Should().HaveCount(5);
    }

    // 12. Son formlar en yeni kayıttan eskiye sıralanır
    [Fact]
    public async Task GetDashboardAsync_RecentForms_OrderedNewestFirst()
    {
        using var db = SqliteTestDatabase.Create();
        var oldest = FormSubmissionFactory.CreateContact("Eski Başvuru");
        var newest = FormSubmissionFactory.CreateContact("Yeni Başvuru");
        db.Context.FormSubmissions.AddRange(oldest, newest);
        await db.Context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        SetCreatedAt(db.Context, oldest, now.AddDays(-1));
        SetCreatedAt(db.Context, newest, now);
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.RecentForms.Select(f => f.FullName).Should().ContainInOrder("Yeni Başvuru", "Eski Başvuru");
    }

    // 13. Son formlar limit (5) kadar döner
    [Fact]
    public async Task GetDashboardAsync_RecentForms_LimitedToFive()
    {
        using var db = SqliteTestDatabase.Create();
        for (var i = 0; i < 7; i++)
        {
            db.Context.FormSubmissions.Add(FormSubmissionFactory.CreateContact($"Kullanıcı {i}"));
        }
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalFormSubmissions.Should().Be(7);
        result.RecentForms.Should().HaveCount(5);
    }

    // 14. Veri olmadığında sıfır sayaç ve boş koleksiyon döner
    [Fact]
    public async Task GetDashboardAsync_WithNoData_ReturnsZeroCountsAndEmptyLists()
    {
        using var db = SqliteTestDatabase.Create();

        var sut = CreateSut(db.Context);
        var result = await sut.GetDashboardAsync();

        result.TotalProducts.Should().Be(0);
        result.ActiveProducts.Should().Be(0);
        result.TotalCategories.Should().Be(0);
        result.TotalCollections.Should().Be(0);
        result.DealerCount.Should().Be(0);
        result.ShowroomCount.Should().Be(0);
        result.TotalUsers.Should().Be(0);
        result.ActiveUsers.Should().Be(0);
        result.TotalFormSubmissions.Should().Be(0);
        result.UnreadFormSubmissions.Should().Be(0);
        result.UnprocessedFormSubmissions.Should().Be(0);
        result.MissingTranslationCount.Should().Be(0);
        result.RecentProducts.Should().BeEmpty();
        result.RecentForms.Should().BeEmpty();
    }

    // 15. Beklenmeyen/null navigation durumlarında (kategorisiz Dealer) servis hata vermez
    [Fact]
    public async Task GetDashboardAsync_WithUnclassifiedDealer_DoesNotThrow_AndExcludesFromBothCounts()
    {
        using var db = SqliteTestDatabase.Create();
        db.Context.Dealers.Add(DealerFactory.CreateUnclassified());
        await db.Context.SaveChangesAsync();

        var sut = CreateSut(db.Context);
        var act = async () => await sut.GetDashboardAsync();

        var result = await act.Should().NotThrowAsync();
        result.Which.DealerCount.Should().Be(0);
        result.Which.ShowroomCount.Should().Be(0);
    }
}
