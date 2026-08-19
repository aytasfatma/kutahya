using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.News;

public class NewsTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "Kısa Açıklama")]
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

public class NewsFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Bülten Kategorisi")]
    public int? NewsCategoryId { get; set; }

    [Display(Name = "Yayın Tarihi")]
    public DateTime? PublishDate { get; set; }

    [Display(Name = "Durum")]
    public NewsStatus Status { get; set; }

    [Display(Name = "İlgili Bültenler")]
    public List<int> SelectedRelatedNewsIds { get; set; } = [];

    public string? ExistingFeaturedImagePath { get; set; }
    public bool RemoveFeaturedImage { get; set; }

    public List<NewsTranslationFieldViewModel> Translations { get; set; } = [];

    public List<SelectListItem> NewsCategoryOptions { get; set; } = [];
    public List<SelectListItem> StatusOptions { get; set; } = [];
    public List<SelectListItem> RelatedNewsOptions { get; set; } = [];
}
