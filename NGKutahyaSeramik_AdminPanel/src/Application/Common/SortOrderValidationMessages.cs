namespace Application.Common;

public static class SortOrderValidationMessages
{
    public const string Required = "Sıralama alanı zorunludur.";
    public const string Minimum = "Sıralama değeri 1 veya daha büyük olmalıdır.";
    public const string Duplicate = "Bu sıralama değeri başka bir kayıt tarafından kullanılıyor. Lütfen farklı bir değer girin.";

    public static bool IsSortOrderMessage(string? message) =>
        message is Minimum or Duplicate;
}
