using Domain.Enums;

namespace Application.Dealers;

public static class DealerEnumDisplay
{
    public static string GetCategoryLabel(DealerCategory? category) => category switch
    {
        DealerCategory.GeneralHeadquarters => "Genel merkez",
        DealerCategory.Factory => "Fabrika",
        DealerCategory.SalesPoint => "Satış noktası",
        null => "Kategorisiz",
        _ => category.ToString()!
    };
}
