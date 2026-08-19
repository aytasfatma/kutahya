using System.ComponentModel.DataAnnotations;
using Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Category;

public class CategoryTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Yüzey Türü Adı")]
    public string? Name { get; set; }

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "SEO URL")]
    public string? SeoUrl { get; set; }

    [Display(Name = "Meta Başlık")]
    public string? MetaTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }
}

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Üst Yüzey Türü")]
    public int? ParentCategoryId { get; set; }

    [Display(Name = "Görsel Yolu")]
    public string? ImagePath { get; set; }
    [Required(ErrorMessage = SortOrderValidationMessages.Required)]
    [Range(1, int.MaxValue, ErrorMessage = SortOrderValidationMessages.Minimum)]
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    public List<CategoryTranslationFieldViewModel> Translations { get; set; } = [];

    public List<SelectListItem> ParentCategoryOptions { get; set; } = [];
}

