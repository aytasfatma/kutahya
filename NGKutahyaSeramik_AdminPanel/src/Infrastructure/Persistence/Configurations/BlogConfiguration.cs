using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Author)
            .HasMaxLength(200);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.IsTrend)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.FeaturedImagePath)
            .HasMaxLength(500);

        builder.Property(b => b.SecondaryImagePath)
            .HasMaxLength(500);

        // BlogCategoryId nullable — kategori silinirse Blog etkilenmez, yalnızca kategorisiz kalır
        // (Restrict değil SetNull; Category/Collection'ın zorunlu-FK Restrict deseninden bilinçli farklı,
        // çünkü Blog.BlogCategoryId doğası gereği opsiyonel).
        builder.HasOne(b => b.BlogCategory)
            .WithMany()
            .HasForeignKey(b => b.BlogCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
