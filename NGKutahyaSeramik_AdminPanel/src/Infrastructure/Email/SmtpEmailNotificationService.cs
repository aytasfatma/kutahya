using System.Net;
using System.Net.Mail;
using System.Text;
using Application.Forms;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Email;
public sealed class SmtpEmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration; public SmtpEmailNotificationService(IConfiguration configuration) => _configuration = configuration;
    public async Task SendCareerApplicationAsync(CareerEmailRequest request, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"]; var username = _configuration["Smtp:Username"]; var password = _configuration["Smtp:Password"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SMTP ayarları tamamlanmamış.");
        using var message = new MailMessage { From = new MailAddress(username, request.FullName, Encoding.UTF8), Subject = $"Kariyer Başvurusu - {request.FullName}", Body = $"Ad Soyad: {request.FullName}\nE-posta: {request.ReplyTo}\nTelefon: {request.Phone}\nDepartman: {request.Department}\n\nMesaj:\n{request.Message}", IsBodyHtml = false };
        message.To.Add(request.Recipient); message.ReplyToList.Add(new MailAddress(request.ReplyTo, request.FullName, Encoding.UTF8));
        if (request.Attachment is { Length: > 0 } && request.AttachmentName is not null) message.Attachments.Add(new Attachment(new MemoryStream(request.Attachment), request.AttachmentName, request.AttachmentContentType));
        using var client = new SmtpClient(host, _configuration.GetValue("Smtp:Port", 587)) { EnableSsl = _configuration.GetValue("Smtp:UseTls", true), Credentials = new NetworkCredential(username, password) };
        cancellationToken.ThrowIfCancellationRequested(); await client.SendMailAsync(message, cancellationToken);
    }
}
