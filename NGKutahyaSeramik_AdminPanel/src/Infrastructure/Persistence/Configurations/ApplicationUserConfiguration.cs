using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>Task 16 — yalnızca IsActive kolonu için (Identity'nin kendi şema alanlarına dokunulmaz).
/// HasDefaultValue(true) — projenin diğer bool "aktif" alanlarıyla (Dealer/Banner/NewsCategory)
/// tutarlı, migration'ın mevcut satırları da geriye dönük true ile doldurmasını garanti eder.</summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
