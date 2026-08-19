using System.ComponentModel.DataAnnotations;
using Application.Common;

namespace Presentation.Models.Banner;

public class BannerTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "Alt Başlık")]
    public string? Subtitle { get; set; }

    [Display(Name = "CTA Buton Metni")]
    public string? ButtonText { get; set; }

    [Display(Name = "CTA Buton Linki")]
    public string? ButtonUrl { get; set; }
}

public class BannerFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Yayın Başlangıç Tarihi")]
    public DateTime? PublishStartDate { get; set; }

    [Display(Name = "Yayın Bitiş Tarihi")]
    public DateTime? PublishEndDate { get; set; }
    [Required(ErrorMessage = SortOrderValidationMessages.Required)]
    [Range(1, int.MaxValue, ErrorMessage = SortOrderValidationMessages.Minimum)]
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    public string? ExistingImagePath { get; set; }
    public bool RemoveImage { get; set; }

    public List<BannerTranslationFieldViewModel> Translations { get; set; } = [];
}

