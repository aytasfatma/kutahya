using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NewsCategoryConfiguration : IEntityTypeConfiguration<NewsCategory>
{
    public void Configure(EntityTypeBuilder<NewsCategory> builder)
    {
        builder.ToTable("NewsCategories");

        builder.HasKey(nc => nc.Id);

        builder.Property(nc => nc.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(nc => nc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(nc => nc.DisplayOrder)
            .IsUnique()
            .HasDatabaseName("UX_NewsCategories_DisplayOrder");
    }
}
