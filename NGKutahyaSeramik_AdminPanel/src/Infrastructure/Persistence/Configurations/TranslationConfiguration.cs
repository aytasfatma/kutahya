using Domain.Entities;
using Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable("Translations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.EntityType)
            .IsRequired()
            .HasConversion(new EntityTypeConverter())
            .HasMaxLength(50)
            .HasColumnType("nvarchar(50)");

        builder.Property(t => t.EntityId)
            .IsRequired();

        builder.Property(t => t.LanguageId)
            .IsRequired();

        builder.Property(t => t.FieldName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Value)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(t => new { t.EntityType, t.EntityId, t.LanguageId, t.FieldName })
            .IsUnique()
            .HasDatabaseName("IX_Translations_Entity_Language_Field");

        builder.HasOne(t => t.Language)
            .WithMany(l => l.Translations)
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
