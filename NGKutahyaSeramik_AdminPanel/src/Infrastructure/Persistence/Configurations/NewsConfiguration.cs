using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.ToTable("News");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.FeaturedImagePath)
            .HasMaxLength(500);

        // NewsCategoryId nullable — kategori silinirse haber etkilenmez, yalnızca kategorisiz kalır
        // (Blog.BlogCategoryId ile aynı SetNull deseni).
        builder.HasOne(n => n.NewsCategory)
            .WithMany()
            .HasForeignKey(n => n.NewsCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
