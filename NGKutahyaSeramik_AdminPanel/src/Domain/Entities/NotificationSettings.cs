namespace Domain.Entities;

public class NotificationSettings
{
    public int Id { get; private set; }
    public string CareerRecipientEmail { get; private set; } = "mratdrn@gmail.com";
    public bool CareerEmailEnabled { get; private set; } = true;
    public DateTime UpdatedAt { get; private set; }
    private NotificationSettings() { }
    public NotificationSettings(string recipient) { CareerRecipientEmail = recipient; CareerEmailEnabled = true; UpdatedAt = DateTime.UtcNow; }
    public void Update(string recipient, bool enabled) { CareerRecipientEmail = recipient.Trim(); CareerEmailEnabled = enabled; UpdatedAt = DateTime.UtcNow; }
}
