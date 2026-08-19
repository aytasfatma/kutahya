namespace Application.Common;

public static class SortOrderValidation
{
    public static bool HasDuplicate<T>(
        IEnumerable<T> items,
        int displayOrder,
        int? excludeId,
        Func<T, int> idSelector,
        Func<T, int> displayOrderSelector) =>
        items.Any(item =>
            (!excludeId.HasValue || idSelector(item) != excludeId.Value) &&
            displayOrderSelector(item) == displayOrder);

    public static int Next<T>(IEnumerable<T> items, Func<T, int> displayOrderSelector)
    {
        var materialized = items as IReadOnlyCollection<T> ?? items.ToList();
        return materialized.Count == 0 ? 1 : materialized.Max(displayOrderSelector) + 1;
    }
}
