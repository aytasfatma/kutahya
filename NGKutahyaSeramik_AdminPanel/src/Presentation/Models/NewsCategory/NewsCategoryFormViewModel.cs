using System.ComponentModel.DataAnnotations;
using Application.Common;

namespace Presentation.Models.NewsCategory;

public class NewsCategoryTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Kategori Adı")]
    public string? Name { get; set; }
}

public class NewsCategoryFormViewModel
{
    public int? Id { get; set; }
    [Required(ErrorMessage = SortOrderValidationMessages.Required)]
    [Range(1, int.MaxValue, ErrorMessage = SortOrderValidationMessages.Minimum)]
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    public List<NewsCategoryTranslationFieldViewModel> Translations { get; set; } = [];
}

