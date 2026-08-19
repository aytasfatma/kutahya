using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Surface> Surfaces => Set<Surface>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();
    public DbSet<CollectionDocument> CollectionDocuments => Set<CollectionDocument>();
    public DbSet<ReferenceProject> ReferenceProjects => Set<ReferenceProject>();
    public DbSet<ReferenceProjectImage> ReferenceProjectImages => Set<ReferenceProjectImage>();
    public DbSet<ProductReferenceProject> ProductReferenceProjects => Set<ProductReferenceProject>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<BlogTag> BlogTags => Set<BlogTag>();
    public DbSet<BlogRelatedPost> BlogRelatedPosts => Set<BlogRelatedPost>();
    public DbSet<News> News => Set<News>();
    public DbSet<NewsCategory> NewsCategories => Set<NewsCategory>();
    public DbSet<NewsRelatedPost> NewsRelatedPosts => Set<NewsRelatedPost>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageContentBlock> PageContentBlocks => Set<PageContentBlock>();
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<DealerImage> DealerImages => Set<DealerImage>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
