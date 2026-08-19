using Domain.Enums;

namespace Application.Forms;

/// <summary>Form kayıtları zamanla büyüyen bir veri seti (Madde 17.2) — bu yüzden filtreleme ve
/// sayfalama repository'de gerçek SQL sorgusu olarak uygulanıyor, tüm kayıtlar belleğe alınmıyor.</summary>
public class FormSubmissionQuery
{
    public FormType? FormType { get; init; }
    public bool? IsRead { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
