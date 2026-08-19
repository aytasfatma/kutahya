using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Persistence.Configurations;
public sealed class NotificationSettingsConfiguration : IEntityTypeConfiguration<NotificationSettings>
{
    public void Configure(EntityTypeBuilder<NotificationSettings> builder)
    {
        builder.ToTable("NotificationSettings"); builder.HasKey(x => x.Id);
        builder.Property(x => x.CareerRecipientEmail).IsRequired().HasMaxLength(320);
        builder.Property(x => x.CareerEmailEnabled).IsRequired(); builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
