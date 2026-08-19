using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using NGKutahyaSeramik.UnitTests.Common;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.IntegrationTests.Database;

/// <summary>
/// EF Core'un gerçek ilişkisel davranışlarını (FK delete behavior, unique constraint) SQLite
/// in-memory üzerinde sınar. `UseInMemoryDatabase` KULLANILMAZ — o sağlayıcı bu constraint'lerin
/// hiçbirini uygulamaz ve burada test edilen senaryoların büyük kısmı yanlış-pozitif geçerdi.
/// </summary>
public class RelationalConstraintTests
{
    [Fact]
    public async Task DeletingBlogCategory_SetsBlogCategoryIdToNull_BlogSurvives()
    {
        using var db = SqliteTestDatabase.Create();

        var category = BlogCategoryFactory.CreateValid();
        db.Context.BlogCategories.Add(category);
        await db.Context.SaveChangesAsync();

        var blog = BlogFactory.CreateDraft(category.Id);
        db.Context.Blogs.Add(blog);
        await db.Context.SaveChangesAsync();

        db.Context.BlogCategories.Remove(category);
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var survivingBlog = await verifyContext.Blogs.FindAsync(blog.Id);

        survivingBlog.Should().NotBeNull();
        survivingBlog!.BlogCategoryId.Should().BeNull();
    }

    [Fact]
    public async Task DeletingNewsCategory_SetsNewsCategoryIdToNull_NewsSurvives()
    {
        using var db = SqliteTestDatabase.Create();

        var category = NewsCategoryFactory.CreateValid();
        db.Context.NewsCategories.Add(category);
        await db.Context.SaveChangesAsync();

        var news = NewsFactory.CreateDraft(category.Id);
        db.Context.News.Add(news);
        await db.Context.SaveChangesAsync();

        db.Context.NewsCategories.Remove(category);
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var survivingNews = await verifyContext.News.FindAsync(news.Id);

        survivingNews.Should().NotBeNull();
        survivingNews!.NewsCategoryId.Should().BeNull();
    }

    [Fact]
    public async Task Dealer_CategoryEnumAndNullCategory_PersistCorrectlyAcrossReload()
    {
        using var db = SqliteTestDatabase.Create();

        var dealer = DealerFactory.CreateDealer();
        var showroom = DealerFactory.CreateShowroom();
        var unclassified = DealerFactory.CreateUnclassified();
        db.Context.Dealers.AddRange(dealer, showroom, unclassified);
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var reloadedDealer = await verifyContext.Dealers.FindAsync(dealer.Id);
        var reloadedShowroom = await verifyContext.Dealers.FindAsync(showroom.Id);
        var reloadedUnclassified = await verifyContext.Dealers.FindAsync(unclassified.Id);

        reloadedDealer!.Category.Should().Be(Domain.Enums.DealerCategory.SalesPoint);
        reloadedShowroom!.Category.Should().Be(Domain.Enums.DealerCategory.Factory);
        reloadedUnclassified!.Category.Should().BeNull();
    }

    [Fact]
    public async Task FormSubmission_TypeSpecificFieldsAndEnum_PersistCorrectlyAcrossReload()
    {
        using var db = SqliteTestDatabase.Create();

        var contact = FormSubmissionFactory.CreateContact();
        var sampleRequest = FormSubmissionFactory.CreateSampleRequest(quantity: 5);
        db.Context.FormSubmissions.AddRange(contact, sampleRequest);
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var reloadedContact = await verifyContext.FormSubmissions.FindAsync(contact.Id);
        var reloadedSample = await verifyContext.FormSubmissions.FindAsync(sampleRequest.Id);

        reloadedContact!.FormType.Should().Be(Domain.Enums.FormType.Contact);
        reloadedContact.Subject.Should().NotBeNull();
        reloadedContact.Address.Should().BeNull();

        reloadedSample!.FormType.Should().Be(Domain.Enums.FormType.SampleRequest);
        reloadedSample.Quantity.Should().Be(5);
        reloadedSample.Subject.Should().BeNull();
    }

    [Fact]
    public async Task ApplicationUser_IsActive_PersistsCorrectlyAcrossReload()
    {
        using var db = SqliteTestDatabase.Create();

        var activeUser = new ApplicationUser { UserName = "active@test.local", Email = "active@test.local", IsActive = true };
        var inactiveUser = new ApplicationUser { UserName = "inactive@test.local", Email = "inactive@test.local", IsActive = false };
        db.Context.Users.AddRange(activeUser, inactiveUser);
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var reloadedActive = await verifyContext.Users.FindAsync(activeUser.Id);
        var reloadedInactive = await verifyContext.Users.FindAsync(inactiveUser.Id);

        reloadedActive!.IsActive.Should().BeTrue();
        reloadedInactive!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeletingPage_CascadesDeleteToAllContentBlocks()
    {
        using var db = SqliteTestDatabase.Create();

        var page = PageFactory.CreateValid();
        db.Context.Pages.Add(page);
        await db.Context.SaveChangesAsync();

        db.Context.PageContentBlocks.Add(PageContentBlockFactory.CreateAccordionBlock(page.Id, 0));
        db.Context.PageContentBlocks.Add(PageContentBlockFactory.CreateTabBlock(page.Id, 1));
        await db.Context.SaveChangesAsync();

        db.Context.Pages.Remove(page);
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var remainingBlocks = await verifyContext.PageContentBlocks.Where(b => b.PageId == page.Id).ToListAsync();

        remainingBlocks.Should().BeEmpty();
    }

    [Fact]
    public async Task Page_SupportsMultipleContentBlocks_OneToMany()
    {
        using var db = SqliteTestDatabase.Create();

        var page = PageFactory.CreateValid();
        db.Context.Pages.Add(page);
        await db.Context.SaveChangesAsync();

        db.Context.PageContentBlocks.Add(PageContentBlockFactory.CreateAccordionBlock(page.Id, 0));
        db.Context.PageContentBlocks.Add(PageContentBlockFactory.CreateTabBlock(page.Id, 1));
        db.Context.PageContentBlocks.Add(PageContentBlockFactory.CreateVideoBlock(page.Id, displayOrder: 2));
        await db.Context.SaveChangesAsync();

        using var verifyContext = db.NewContext();
        var blocks = await verifyContext.PageContentBlocks.Where(b => b.PageId == page.Id).ToListAsync();

        blocks.Should().HaveCount(3);
        blocks.Select(b => b.PageId).Distinct().Should().ContainSingle().Which.Should().Be(page.Id);
    }

    [Fact]
    public async Task DuplicateProductCode_ViolatesUniqueConstraint()
    {
        using var db = SqliteTestDatabase.Create();

        var category = CategoryFactory.CreateRoot();
        var collection = CollectionFactory.CreateValid();
        db.Context.Categories.Add(category);
        db.Context.Collections.Add(collection);
        await db.Context.SaveChangesAsync();

        db.Context.Products.Add(ProductFactory.CreateValid("DUP001RP", category.Id, collection.Id));
        await db.Context.SaveChangesAsync();

        db.Context.Products.Add(ProductFactory.CreateValid("DUP001RP", category.Id, collection.Id));
        var act = async () => await db.Context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateTagName_ViolatesUniqueConstraint()
    {
        using var db = SqliteTestDatabase.Create();

        db.Context.Tags.Add(new Tag("Banyo"));
        await db.Context.SaveChangesAsync();

        db.Context.Tags.Add(new Tag("Banyo"));
        var act = async () => await db.Context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateBlogTagAssociation_ViolatesUniqueConstraint()
    {
        using var db = SqliteTestDatabase.Create();

        var blog = BlogFactory.CreateDraft();
        var tag = new Tag("Mutfak");
        db.Context.Blogs.Add(blog);
        db.Context.Tags.Add(tag);
        await db.Context.SaveChangesAsync();

        db.Context.BlogTags.Add(new BlogTag(blog.Id, tag.Id));
        await db.Context.SaveChangesAsync();

        db.Context.BlogTags.Add(new BlogTag(blog.Id, tag.Id));
        var act = async () => await db.Context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateTranslationEntry_ViolatesCompositeUniqueConstraint()
    {
        using var db = SqliteTestDatabase.Create();

        var language = db.Context.Languages.Add(new Language("TR", "Türkçe", isActive: true, displayOrder: 1)).Entity;
        await db.Context.SaveChangesAsync();

        db.Context.Translations.Add(new Translation(EntityType.Page, entityId: 1, language.Id, "Title", "Değer 1"));
        await db.Context.SaveChangesAsync();

        db.Context.Translations.Add(new Translation(EntityType.Page, entityId: 1, language.Id, "Title", "Değer 2"));
        var act = async () => await db.Context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeletingLanguageInUse_IsRestricted()
    {
        using var db = SqliteTestDatabase.Create();

        var language = db.Context.Languages.Add(new Language("RU", "Русский", isActive: true, displayOrder: 7)).Entity;
        await db.Context.SaveChangesAsync();

        db.Context.Translations.Add(new Translation(EntityType.Page, entityId: 1, language.Id, "Title", "Значение"));
        await db.Context.SaveChangesAsync();

        // Taze, izlemesiz bir context ile silme denenir — bu sayede EF'in change tracker'ı (aynı
        // context'te hâlâ izlenen Translation nesnesi yüzünden) client-side erken uyarı vermez;
        // gerçek DB round-trip'i zorlanır ve "ON DELETE RESTRICT" (ADR-012) doğrudan SQL seviyesinde
        // sınanır.
        using var freshContext = db.NewContext();
        var languageToDelete = await freshContext.Languages.SingleAsync(l => l.Code == "RU");
        freshContext.Languages.Remove(languageToDelete);
        var act = async () => await freshContext.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>("Translation.LanguageId FK'si ON DELETE RESTRICT (ADR-012)");
    }
}
