namespace Application.Forms;

public sealed record NotificationSettingsDto(string CareerRecipientEmail, bool CareerEmailEnabled);
public interface INotificationSettingsRepository
{
    Task<Domain.Entities.NotificationSettings?> GetAsync();
    Task AddAsync(Domain.Entities.NotificationSettings settings);
}
public sealed class NotificationSettingsService
{
    private readonly INotificationSettingsRepository _repository; private readonly IUnitOfWork _unit;
    public NotificationSettingsService(INotificationSettingsRepository repository, IUnitOfWork unit) { _repository = repository; _unit = unit; }
    public async Task<NotificationSettingsDto> GetAsync() { var x = await _repository.GetAsync(); return new(x?.CareerRecipientEmail ?? "mratdrn@gmail.com", x?.CareerEmailEnabled ?? true); }
    public async Task UpdateAsync(string email, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) throw new ArgumentException("Geçerli bir alıcı e-posta adresi girin.");
        var x = await _repository.GetAsync(); if (x is null) await _repository.AddAsync(new Domain.Entities.NotificationSettings(email)); else x.Update(email, enabled);
        await _unit.SaveChangesAsync();
    }
}

public sealed record CareerEmailRequest(string Recipient, string FullName, string ReplyTo, string Phone, string Department,
    string Message, string? AttachmentName, string? AttachmentContentType, byte[]? Attachment);
public interface IEmailNotificationService
{
    Task SendCareerApplicationAsync(CareerEmailRequest request, CancellationToken cancellationToken = default);
}
