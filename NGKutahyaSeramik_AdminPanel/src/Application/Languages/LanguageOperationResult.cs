namespace Application.Languages;

public class LanguageOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static LanguageOperationResult Success() => new() { Succeeded = true };

    public static LanguageOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
