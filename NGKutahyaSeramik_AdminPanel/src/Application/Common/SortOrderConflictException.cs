namespace Application.Common;

public sealed class SortOrderConflictException : Exception
{
    public SortOrderConflictException() : base(SortOrderValidationMessages.Duplicate)
    {
    }
}
