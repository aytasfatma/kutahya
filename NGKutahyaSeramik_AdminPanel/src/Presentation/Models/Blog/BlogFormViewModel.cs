using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Blog;

public class BlogTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "Kısa Özet")]
    public string? Excerpt { get; set; }

    [Display(Name = "İçerik")]
    public string? Content { get; set; }

    [Display(Name = "SEO URL")]
    public string? SeoUrl { get; set; }

    [Display(Name = "Meta Başlık")]
    public string? MetaTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }
}

public class BlogFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Blog Kategorisi")]
    public int? BlogCategoryId { get; set; }

    [Display(Name = "Yazar")]
    public string? Author { get; set; }

    [Display(Name = "Yayın Tarihi")]
    public DateTime? PublishDate { get; set; }

    [Display(Name = "Durum")]
    public BlogStatus Status { get; set; }

    [Display(Name = "TREND blog kategorisine taşıyın")]
    public bool IsTrend { get; set; }

    [Display(Name = "Etiketler (virgülle ayırın)")]
    public string? TagsInput { get; set; }

    [Display(Name = "Benzer Hikâyeler")]
    public List<int> SelectedRelatedBlogIds { get; set; } = [];

    public string? ExistingFeaturedImagePath { get; set; }
    public bool RemoveFeaturedImage { get; set; }
    public string? ExistingSecondaryImagePath { get; set; }
    public bool RemoveSecondaryImage { get; set; }

    public List<BlogTranslationFieldViewModel> Translations { get; set; } = [];

    public List<SelectListItem> BlogCategoryOptions { get; set; } = [];
    public List<SelectListItem> StatusOptions { get; set; } = [];
    public List<SelectListItem> RelatedBlogOptions { get; set; } = [];
}
