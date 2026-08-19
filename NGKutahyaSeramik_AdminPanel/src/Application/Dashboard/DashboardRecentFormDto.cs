using Domain.Enums;
using Application.Forms;

namespace Application.Dashboard;

public class DashboardRecentFormDto
{
    public int Id { get; init; }
    public FormType FormType { get; init; }
    public string FullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public bool IsRead { get; init; }

    public string FormTypeLabel => FormEnumDisplay.GetFormTypeLabel(FormType);
}
