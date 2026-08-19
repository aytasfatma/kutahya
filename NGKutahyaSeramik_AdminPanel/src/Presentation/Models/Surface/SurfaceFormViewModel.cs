using System.ComponentModel.DataAnnotations;
using Presentation.Models.Collection;

namespace Presentation.Models.Surface;

public class SurfaceFormViewModel
{
    public int? Id { get; set; }
    [Required, Display(Name = "Yüzey Adı")]
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public List<CollectionTranslationFieldViewModel> Translations { get; set; } = [];
    [Range(1, int.MaxValue), Display(Name = "Sıralama")]
    public int DisplayOrder { get; set; } = 1;
}
