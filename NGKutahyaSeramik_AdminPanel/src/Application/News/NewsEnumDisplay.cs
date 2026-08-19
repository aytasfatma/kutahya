using Domain.Enums;

namespace Application.News;

public static class NewsEnumDisplay
{
    public static string GetStatusLabel(NewsStatus status) => status switch
    {
        NewsStatus.Draft => "Taslak",
        NewsStatus.Published => "Yayında",
        NewsStatus.Archived => "Arşiv",
        _ => status.ToString()
    };
}
