namespace Application.Forms;

public class FormSubmissionOperationResult
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static FormSubmissionOperationResult Success() => new() { Succeeded = true };

    public static FormSubmissionOperationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
