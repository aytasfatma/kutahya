using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.SeoUrl).HasMaxLength(300);

        builder.Property(c => c.ImagePath)
            .HasMaxLength(500);

        builder.Property(c => c.BrandCodes).HasMaxLength(100).IsRequired();
        builder.Ignore(c => c.Brands);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(c => c.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_Collections_DisplayOrder");
    }
}
