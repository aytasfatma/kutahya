using System.Text.RegularExpressions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Forms;

public partial class FormSubmissionService
{
    private const int MaxShortFieldLength = 200;
    private const int MaxMessageLength = 4000;

    private readonly IFormSubmissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FormSubmissionService(IFormSubmissionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<FormSubmissionDto>> GetPagedAsync(FormSubmissionQuery query)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(query);

        return new PagedResult<FormSubmissionDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<FormSubmissionDto?> GetByIdAsync(int id)
    {
        var submission = await _repository.GetByIdAsync(id);
        return submission is null ? null : MapToDto(submission);
    }

    /// <summary>Bu fazda hiçbir public/anonim controller'dan çağrılmıyor (ADR-001/002/009) —
    /// gelecekteki public form gönderimi için Application katmanında hazır tutuluyor.</summary>
    public async Task<FormSubmissionOperationResult> CreateSubmissionAsync(CreateFormSubmissionRequest request)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return FormSubmissionOperationResult.Failure(validation);
        }

        var submission = new FormSubmission(
            request.FormType,
            request.FullName.Trim(),
            request.Email.Trim(),
            request.Phone.Trim(),
            Normalize(request.Company),
            request.Message.Trim(),
            request.ConsentAccepted,
            Normalize(request.Subject),
            Normalize(request.ProductCode),
            Normalize(request.ProductName),
            Normalize(request.Address),
            Normalize(request.RequestedProduct),
            request.Quantity);

        await _repository.AddAsync(submission);
        await _unitOfWork.SaveChangesAsync();

        return FormSubmissionOperationResult.Success();
    }

    public async Task<FormSubmissionOperationResult> MarkAsReadAsync(int id)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null)
        {
            return FormSubmissionOperationResult.Failure("Form başvurusu bulunamadı.");
        }

        submission.MarkAsRead();
        await _unitOfWork.SaveChangesAsync();
        return FormSubmissionOperationResult.Success();
    }

    public async Task<FormSubmissionOperationResult> MarkAsUnreadAsync(int id)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null)
        {
            return FormSubmissionOperationResult.Failure("Form başvurusu bulunamadı.");
        }

        submission.MarkAsUnread();
        await _unitOfWork.SaveChangesAsync();
        return FormSubmissionOperationResult.Success();
    }

    public async Task<FormSubmissionOperationResult> MarkAsProcessedAsync(int id)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null)
        {
            return FormSubmissionOperationResult.Failure("Form başvurusu bulunamadı.");
        }

        submission.MarkAsProcessed();
        await _unitOfWork.SaveChangesAsync();
        return FormSubmissionOperationResult.Success();
    }

    public async Task<FormSubmissionOperationResult> MarkAsUnprocessedAsync(int id)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null)
        {
            return FormSubmissionOperationResult.Failure("Form başvurusu bulunamadı.");
        }

        submission.MarkAsUnprocessed();
        await _unitOfWork.SaveChangesAsync();
        return FormSubmissionOperationResult.Success();
    }

    public async Task<FormSubmissionOperationResult> UpdateAdminNoteAsync(int id, string? note)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null)
        {
            return FormSubmissionOperationResult.Failure("Form başvurusu bulunamadı.");
        }

        if (note is not null && note.Length > MaxMessageLength)
        {
            return FormSubmissionOperationResult.Failure($"Not en fazla {MaxMessageLength} karakter olabilir.");
        }

        submission.UpdateAdminNote(Normalize(note));
        await _unitOfWork.SaveChangesAsync();
        return FormSubmissionOperationResult.Success();
    }

    public async Task<FormSubmissionOperationResult> DeleteAsync(int id)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null)
        {
            return FormSubmissionOperationResult.Failure("Form başvurusu bulunamadı.");
        }

        _repository.Remove(submission);
        await _unitOfWork.SaveChangesAsync();
        return FormSubmissionOperationResult.Success();
    }

    private static string? Validate(CreateFormSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return "Ad soyad zorunludur.";
        }

        if (request.FullName.Length > MaxShortFieldLength)
        {
            return $"Ad soyad en fazla {MaxShortFieldLength} karakter olabilir.";
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex().IsMatch(request.Email.Trim()))
        {
            return "Geçerli bir e-posta adresi zorunludur.";
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            return "Telefon zorunludur.";
        }

        if (request.Phone.Length > 50)
        {
            return "Telefon en fazla 50 karakter olabilir.";
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return "Mesaj zorunludur.";
        }

        if (request.Message.Length > MaxMessageLength)
        {
            return $"Mesaj en fazla {MaxMessageLength} karakter olabilir.";
        }

        if (!request.ConsentAccepted)
        {
            return "KVKK onayı zorunludur.";
        }

        return request.FormType switch
        {
            FormType.Contact => string.IsNullOrWhiteSpace(request.Subject) ? "Konu zorunludur." : null,
            FormType.RequestInformation => string.IsNullOrWhiteSpace(request.ProductCode) || string.IsNullOrWhiteSpace(request.ProductName)
                ? "Ürün kodu ve ürün adı zorunludur."
                : null,
            FormType.SampleRequest => ValidateSampleRequest(request),
            _ => "Geçersiz form türü."
        };
    }

    private static string? ValidateSampleRequest(CreateFormSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return "Adres zorunludur.";
        }

        if (string.IsNullOrWhiteSpace(request.RequestedProduct))
        {
            return "Talep edilen ürün/koleksiyon zorunludur.";
        }

        if (request.Quantity is null or <= 0)
        {
            return "Adet 0'dan büyük olmalıdır.";
        }

        return null;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static FormSubmissionDto MapToDto(FormSubmission submission) => new()
    {
        Id = submission.Id,
        FormType = submission.FormType,
        FullName = submission.FullName,
        Email = submission.Email,
        Phone = submission.Phone,
        Company = submission.Company,
        Message = submission.Message,
        ConsentAccepted = submission.ConsentAccepted,
        Subject = submission.Subject,
        ProductCode = submission.ProductCode,
        ProductName = submission.ProductName,
        Address = submission.Address,
        RequestedProduct = submission.RequestedProduct,
        Quantity = submission.Quantity,
        IsRead = submission.IsRead,
        ReadAt = submission.ReadAt,
        ProcessedAt = submission.ProcessedAt,
        AdminNote = submission.AdminNote,
        CreatedAt = submission.CreatedAt
    };

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
