using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Brand)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(d => d.FileExtension)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.Property(d => d.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Diller silinemediği için (ON DELETE RESTRICT, ADR-012) burada da Restrict tutarlı.
        builder.HasOne(d => d.Language)
            .WithMany()
            .HasForeignKey(d => d.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.DocumentType, d.LanguageId, d.DisplayOrder })
            .IsUnique()
            .HasDatabaseName("UX_Documents_DocumentType_LanguageId_DisplayOrder");
    }
}
