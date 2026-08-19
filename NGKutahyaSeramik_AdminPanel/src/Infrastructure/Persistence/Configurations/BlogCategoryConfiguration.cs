using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable("BlogCategories");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(bc => bc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(bc => bc.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_BlogCategories_DisplayOrder");
    }
}
