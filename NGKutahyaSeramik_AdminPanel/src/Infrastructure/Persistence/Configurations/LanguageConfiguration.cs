using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnType("nvarchar(10)");

        builder.HasIndex(l => l.Code)
            .IsUnique()
            .HasDatabaseName("IX_Languages_Code");

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasIndex(l => l.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_Languages_DisplayOrder");
    }
}
