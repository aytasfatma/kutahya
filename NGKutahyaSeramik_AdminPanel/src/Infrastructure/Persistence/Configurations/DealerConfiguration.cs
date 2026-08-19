using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DealerConfiguration : IEntityTypeConfiguration<Dealer>
{
    public void Configure(EntityTypeBuilder<Dealer> builder)
    {
        builder.ToTable("Dealers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.City)
            .IsRequired()
            .HasMaxLength(100);

        // Madde 25.1: "Category enum, Bayi (2), Showroom (3)" — 17 kategorisiz kayıt gerçeğini
        // yansıtmak için nullable (bkz. Dealer.cs XML doc).
        builder.Property(d => d.Category)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.District).HasMaxLength(100);
        builder.Property(d => d.Address).HasMaxLength(500);
        builder.Property(d => d.Phone).HasMaxLength(50);
        builder.Property(d => d.Fax).HasMaxLength(50);
        builder.Property(d => d.Email).HasMaxLength(200);
        builder.Property(d => d.WorkingHours).HasMaxLength(250);
        builder.Property(d => d.Region).HasMaxLength(20);
        builder.Property(d => d.RegionName).HasMaxLength(100);

        // -90..90 / -180..180 aralığı service seviyesinde doğrulanıyor; kolon precision'ı
        // (9,6) santimetre-altı hassasiyet sağlıyor, float/double kullanılmadı.
        builder.Property(d => d.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(d => d.Longitude).HasColumnType("decimal(9,6)");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.BrandCodes).HasMaxLength(100).IsRequired();
        builder.Ignore(d => d.Brands);
    }
}
