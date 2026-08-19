using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("FormSubmissions");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FormType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(f => f.FullName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Email).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Phone).IsRequired().HasMaxLength(50);
        builder.Property(f => f.Company).HasMaxLength(200);
        builder.Property(f => f.Message).IsRequired().HasMaxLength(4000);

        builder.Property(f => f.Subject).HasMaxLength(200);
        builder.Property(f => f.ProductCode).HasMaxLength(50);
        builder.Property(f => f.ProductName).HasMaxLength(200);
        builder.Property(f => f.Address).HasMaxLength(500);
        builder.Property(f => f.RequestedProduct).HasMaxLength(200);

        builder.Property(f => f.AdminNote).HasMaxLength(4000);

        builder.Property(f => f.IsRead).IsRequired().HasDefaultValue(false);
        builder.Property(f => f.CreatedAt).IsRequired();

        // Her Index sayfası yüklemesi (filtre olsun olmasın) CreatedAt DESC'e göre sıralanıp
        // sayfalanıyor (bkz. FormSubmissionRepository.GetPagedAsync) — bu yüzden en değerli tekil
        // index bu.
        builder.HasIndex(f => f.CreatedAt)
            .HasDatabaseName("IX_FormSubmissions_CreatedAt");

        // Admin panelinde "form türüne göre filtrele" gerçek bir sorgu deseni (görev talimatı +
        // Madde 12) — composite index hem filtre hem sıralamayı kapsıyor.
        builder.HasIndex(f => new { f.FormType, f.CreatedAt })
            .HasDatabaseName("IX_FormSubmissions_FormType_CreatedAt");

        // IsRead düşük kardinaliteli bool — bu veri ölçeğinde ayrı bir index eklenmedi (bkz. kapanış
        // raporu Index Review). Email/Phone/arama alanlarına da index eklenmedi — LIKE '%term%'
        // aramasının normal index'ten faydası sınırlı, full-text search bu task kapsamında değil.
    }
}
