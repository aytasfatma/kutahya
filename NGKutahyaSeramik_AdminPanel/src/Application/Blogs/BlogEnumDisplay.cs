using Domain.Enums;

namespace Application.Blogs;

public static class BlogEnumDisplay
{
    public static string GetStatusLabel(BlogStatus status) => status switch
    {
        BlogStatus.Draft => "Taslak",
        BlogStatus.Published => "Yayında",
        BlogStatus.Archived => "Arşiv",
        _ => status.ToString()
    };
}
