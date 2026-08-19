using Application.Forms;
using Domain.Enums;

namespace Presentation.Models.FormSubmission;

public class FormSubmissionIndexViewModel
{
    public PagedResult<FormSubmissionDto> Page { get; init; } = new();

    public FormType? FormType { get; init; }
    public bool? IsRead { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public string? SearchTerm { get; init; }
}
