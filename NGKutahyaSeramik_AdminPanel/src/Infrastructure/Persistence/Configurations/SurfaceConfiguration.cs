using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SurfaceConfiguration : IEntityTypeConfiguration<Surface>
{
    public void Configure(EntityTypeBuilder<Surface> builder)
    {
        builder.ToTable("Surfaces");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SeoUrl).HasMaxLength(300);
        builder.Property(x => x.ImagePath).HasMaxLength(500);
        builder.Property(x => x.BrandCodes).IsRequired().HasMaxLength(100);
        builder.Ignore(x => x.Brands);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.DisplayOrder).IsUnique();
    }
}
