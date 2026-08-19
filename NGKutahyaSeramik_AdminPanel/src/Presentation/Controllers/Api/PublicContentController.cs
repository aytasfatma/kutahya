using Application.Banners;
using Application.Blogs;
using Application.News;
using Application.Pages;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Api;

namespace Presentation.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Produces("application/json")]
public sealed class PublicContentController : ControllerBase
{
    private readonly BlogService _blogService;
    private readonly NewsService _newsService;
    private readonly BannerService _bannerService;
    private readonly PageService _pageService;
    private readonly PageContentBlockService _pageContentBlockService;

    public PublicContentController(
        BlogService blogService,
        NewsService newsService,
        BannerService bannerService,
        PageService pageService,
        PageContentBlockService pageContentBlockService)
    {
        _blogService = blogService;
        _newsService = newsService;
        _bannerService = bannerService;
        _pageService = pageService;
        _pageContentBlockService = pageContentBlockService;
    }

    [HttpGet("blogs")]
    public async Task<ActionResult<PublicPagedResponse<PublicBlogListResponse>>> Blogs(
        [FromQuery] string lang = "tr", [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        (page, pageSize) = NormalizePaging(page, pageSize);
        var now = DateTime.UtcNow;
        var localized = (await _blogService.GetAllAsync())
            .Where(x => x.Status == BlogStatus.Published && (!x.PublishDate.HasValue || x.PublishDate <= now))
            .Select(x => (Blog: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .Where(x => x.Translation is not null)
            .OrderByDescending(x => x.Blog.PublishDate)
            .ToList();

        var items = localized.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new PublicBlogListResponse(
                x.Blog.Id, x.Translation!.Title ?? x.Blog.DisplayTitle ?? string.Empty,
                x.Translation.Excerpt, x.Translation.SeoUrl, x.Blog.BlogCategoryName,
                x.Blog.Author, x.Blog.PublishDate, AssetUrl(x.Blog.FeaturedImagePath), x.Blog.Tags, x.Blog.IsTrend))
            .ToList();
        return Ok(new PublicPagedResponse<PublicBlogListResponse>(items, page, pageSize, localized.Count));
    }

    [HttpGet("blogs/{seoUrl}")]
    public async Task<ActionResult<PublicBlogDetailResponse>> Blog(string seoUrl, [FromQuery] string lang = "tr")
    {
        var now = DateTime.UtcNow;
        var allBlogs = await _blogService.GetAllAsync();
        var published = allBlogs.Where(x => x.Status == BlogStatus.Published && (!x.PublishDate.HasValue || x.PublishDate <= now)).ToList();
        var item = published
            .Select(x => (Blog: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .FirstOrDefault(x => string.Equals(x.Translation?.SeoUrl, seoUrl, StringComparison.OrdinalIgnoreCase));
        if (item.Blog is null) return NotFound();

        var relatedPosts = item.Blog.RelatedPosts
            .Where(rp => published.Any(p => p.Id == rp.Id))
            .Select(rp => new PublicRelatedPostResponse(rp.Id, rp.Title, rp.SeoUrl, AssetUrl(rp.FeaturedImagePath)))
            .ToList();

        return Ok(new PublicBlogDetailResponse
        {
            Id = item.Blog.Id, Title = item.Translation?.Title ?? item.Blog.DisplayTitle ?? string.Empty,
            Excerpt = item.Translation?.Excerpt, Content = item.Translation?.Content, SeoUrl = item.Translation?.SeoUrl,
            MetaTitle = item.Translation?.MetaTitle, MetaDescription = item.Translation?.MetaDescription,
            CategoryName = item.Blog.BlogCategoryName, Author = item.Blog.Author,
            PublishDate = item.Blog.PublishDate, FeaturedImageUrl = AssetUrl(item.Blog.FeaturedImagePath), Tags = item.Blog.Tags,
            SecondaryImageUrl = AssetUrl(item.Blog.SecondaryImagePath),
            IsTrend = item.Blog.IsTrend,
            RelatedPosts = relatedPosts
        });
    }

    [HttpGet("news")]
    public async Task<ActionResult<PublicPagedResponse<PublicNewsListResponse>>> News(
        [FromQuery] string lang = "tr", [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        (page, pageSize) = NormalizePaging(page, pageSize);
        var now = DateTime.UtcNow;
        var localized = (await _newsService.GetAllAsync())
            .Where(x => x.Status == NewsStatus.Published && (!x.PublishDate.HasValue || x.PublishDate <= now))
            .Select(x => (News: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .Where(x => x.Translation is not null)
            .OrderByDescending(x => x.News.PublishDate)
            .ToList();

        var items = localized.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new PublicNewsListResponse(
                x.News.Id, x.Translation!.Title ?? x.News.DisplayTitle ?? string.Empty,
                x.Translation.SeoUrl, x.News.NewsCategoryName, x.News.PublishDate,
                AssetUrl(x.News.FeaturedImagePath)))
            .ToList();
        return Ok(new PublicPagedResponse<PublicNewsListResponse>(items, page, pageSize, localized.Count));
    }

    [HttpGet("news/{seoUrl}")]
    public async Task<ActionResult<PublicNewsDetailResponse>> NewsDetail(string seoUrl, [FromQuery] string lang = "tr")
    {
        var now = DateTime.UtcNow;
        var allNews = await _newsService.GetAllAsync();
        var published = allNews.Where(x => x.Status == NewsStatus.Published && (!x.PublishDate.HasValue || x.PublishDate <= now)).ToList();
        var item = published
            .Select(x => (News: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .FirstOrDefault(x => string.Equals(x.Translation?.SeoUrl, seoUrl, StringComparison.OrdinalIgnoreCase));
        if (item.News is null) return NotFound();

        var relatedPosts = item.News.RelatedPosts
            .Where(rp => published.Any(p => p.Id == rp.Id))
            .Select(rp => new PublicRelatedPostResponse(rp.Id, rp.Title, rp.SeoUrl, AssetUrl(rp.FeaturedImagePath)))
            .ToList();

        return Ok(new PublicNewsDetailResponse
        {
            Id = item.News.Id, Title = item.Translation?.Title ?? item.News.DisplayTitle ?? string.Empty,
            Excerpt = item.Translation?.Excerpt,
            Content = item.Translation?.Content, SeoUrl = item.Translation?.SeoUrl,
            MetaTitle = item.Translation?.MetaTitle, MetaDescription = item.Translation?.MetaDescription,
            CategoryName = item.News.NewsCategoryName, PublishDate = item.News.PublishDate,
            FeaturedImageUrl = AssetUrl(item.News.FeaturedImagePath),
            RelatedPosts = relatedPosts
        });
    }

    [HttpGet("banners")]
    public async Task<ActionResult<IReadOnlyList<PublicBannerResponse>>> Banners([FromQuery] string lang = "tr")
    {
        var now = DateTime.UtcNow;
        var items = (await _bannerService.GetAllAsync())
            .Where(x => x.IsActive
                && (!x.PublishStartDate.HasValue || x.PublishStartDate <= now)
                && (!x.PublishEndDate.HasValue || x.PublishEndDate >= now))
            .OrderBy(x => x.DisplayOrder)
            .Select(x =>
            {
                var t = Translation(x.Translations, lang, y => y.LanguageCode);
                return new PublicBannerResponse(x.Id, t?.Title, t?.Subtitle, t?.ButtonText, t?.ButtonUrl,
                    AssetUrl(x.ImagePath), x.PublishStartDate, x.PublishEndDate, x.DisplayOrder);
            }).ToList();
        return Ok(items);
    }

    [HttpGet("pages")]
    public async Task<ActionResult<IReadOnlyList<PublicPageListResponse>>> Pages([FromQuery] string lang = "tr")
    {
        var items = (await _pageService.GetAllAsync())
            .Select(x => (Page: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .Where(x => x.Translation is not null && !string.IsNullOrWhiteSpace(x.Translation.SeoUrl))
            .OrderBy(x => x.Translation!.Title)
            .Select(x => new PublicPageListResponse(x.Page.Id, x.Translation!.Title ?? x.Page.DisplayTitle ?? string.Empty,
                x.Translation.SeoUrl, x.Translation.MetaTitle, x.Translation.MetaDescription, x.Page.UpdatedAt))
            .ToList();
        return Ok(items);
    }

    [HttpGet("pages/{**seoUrl}")]
    public async Task<ActionResult<PublicPageDetailResponse>> Page(string seoUrl, [FromQuery] string lang = "tr")
    {
        var item = (await _pageService.GetAllAsync())
            .Select(x => (Page: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .FirstOrDefault(x => string.Equals(x.Translation?.SeoUrl, seoUrl, StringComparison.OrdinalIgnoreCase));
        if (item.Page is null) return NotFound();

        var blocks = (await _pageContentBlockService.GetByPageIdAsync(item.Page.Id))
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x =>
            {
                var t = Translation(x.Translations, lang, y => y.LanguageCode);
                return new PublicPageBlockResponse(x.Id, x.BlockType.ToString(), x.BlockTypeLabel,
                    t?.Title, t?.Content, AssetUrl(x.ImagePath), x.VideoEmbedUrl, x.DisplayOrder);
            }).ToList();

        return Ok(new PublicPageDetailResponse
        {
            Id = item.Page.Id, Title = item.Translation?.Title ?? item.Page.DisplayTitle ?? string.Empty,
            SeoUrl = item.Translation?.SeoUrl, MetaTitle = item.Translation?.MetaTitle,
            MetaDescription = item.Translation?.MetaDescription, UpdatedAt = item.Page.UpdatedAt, Blocks = blocks
        });
    }

    private string? AssetUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}/{path.TrimStart('/')}";
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize) =>
        (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));

    private static string NormalizeLanguage(string? language) =>
        string.IsNullOrWhiteSpace(language) ? "TR" : language.Trim().ToUpperInvariant();

    private static T? Translation<T>(IEnumerable<T> translations, string language, Func<T, string> languageCode)
        where T : class
    {
        var items = translations.ToList();
        var requested = NormalizeLanguage(language);
        return items.FirstOrDefault(x => string.Equals(languageCode(x), requested, StringComparison.OrdinalIgnoreCase))
            ?? items.FirstOrDefault(x => string.Equals(languageCode(x), "TR", StringComparison.OrdinalIgnoreCase))
            ?? items.FirstOrDefault();
    }
}

