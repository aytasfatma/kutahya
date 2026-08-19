using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.Page;

public class PageTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "SEO URL")]
    public string? SeoUrl { get; set; }

    [Display(Name = "Meta Başlık")]
    public string? MetaTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }
}

public class PageFormViewModel
{
    public int? Id { get; set; }

    public List<PageTranslationFieldViewModel> Translations { get; set; } = [];

    // Alan-seviyeli RBAC (backlog #23) — Controller tarafından role göre set edilir. Varsayılan
    // true: Create ekranı yalnızca Admin/İçerik Editörü'ne açık, orada kısıtlama uygulanmaz.
    public bool CanEditContentFields { get; set; } = true;
    public bool CanEditSeoFields { get; set; } = true;
}
