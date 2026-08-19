using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ImagePath)
            .HasMaxLength(500);

        builder.Property(b => b.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(b => b.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_Banners_DisplayOrder");
    }
}
