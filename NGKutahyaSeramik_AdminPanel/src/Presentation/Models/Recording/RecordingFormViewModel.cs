using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.Recording;

public class RecordingFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Video bağlantısı")]
    public string? VideoUrl { get; set; }

    [Display(Name = "Üst başlık")]
    public string? Eyebrow { get; set; }

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Yayın durumu")]
    public bool IsActive { get; set; } = true;
}

public class RecordingListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? VideoUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
