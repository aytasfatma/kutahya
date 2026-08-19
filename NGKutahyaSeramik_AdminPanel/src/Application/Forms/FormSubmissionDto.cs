using Domain.Enums;

namespace Application.Forms;

public class FormSubmissionDto
{
    public int Id { get; init; }
    public FormType FormType { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Company { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool ConsentAccepted { get; init; }

    public string? Subject { get; init; }
    public string? ProductCode { get; init; }
    public string? ProductName { get; init; }
    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public int? Quantity { get; init; }

    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? AdminNote { get; init; }
    public DateTime CreatedAt { get; init; }

    public string FormTypeLabel => FormEnumDisplay.GetFormTypeLabel(FormType);
    public bool IsProcessed => ProcessedAt.HasValue;
}
