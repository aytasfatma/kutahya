using System.ComponentModel.DataAnnotations;
using Application.Common;

namespace Presentation.Models.Language;

public class LanguageEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Dil Kodu")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Dil Adı")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = SortOrderValidationMessages.Required)]
    [Range(1, int.MaxValue, ErrorMessage = SortOrderValidationMessages.Minimum)]
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    // Backend guardrail'in (LanguageService.UpdateAsync) View'da yansıması — yalnızca UI kozmetiği,
    // gerçek kural her zaman servis katmanında da uygulanıyor.
    public bool IsTurkish => string.Equals(Code, "TR", StringComparison.OrdinalIgnoreCase);
}

