using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.FormSubmission;

public class AdminNoteViewModel
{
    public int Id { get; set; }

    [Display(Name = "Yönetici Notu")]
    public string? AdminNote { get; set; }
}
