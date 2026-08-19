using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.SeoUrl).HasMaxLength(300);

        builder.Property(c => c.ImagePath)
            .HasMaxLength(500);

        builder.Property(c => c.BrandCodes)
            .HasMaxLength(100)
            .IsRequired();
        builder.Ignore(c => c.Brands);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ParentCategoryId, c.DisplayOrder })
            .IsUnique()
            .HasFilter("[ParentCategoryId] IS NOT NULL")
            .HasDatabaseName("UX_Categories_ParentCategoryId_DisplayOrder");

        builder.HasIndex(c => c.DisplayOrder)
            .IsUnique()
            .HasFilter("[ParentCategoryId] IS NULL")
            .HasDatabaseName("UX_Categories_Root_DisplayOrder");
    }
}
