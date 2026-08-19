using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Page;

public class PageContentBlockTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "İçerik")]
    public string? Content { get; set; }
}

public class PageContentBlockFormViewModel
{
    public int PageId { get; set; }
    public int? Id { get; set; }

    [Display(Name = "Blok Tipi")]
    public PageBlockType BlockType { get; set; }

    [Display(Name = "Video Embed Linki")]
    public string? VideoEmbedUrl { get; set; }

    public string? ExistingImagePath { get; set; }
    public bool RemoveImage { get; set; }

    public List<SelectListItem> BlockTypeOptions { get; set; } = [];
    public List<PageContentBlockTranslationFieldViewModel> Translations { get; set; } = [];
}
