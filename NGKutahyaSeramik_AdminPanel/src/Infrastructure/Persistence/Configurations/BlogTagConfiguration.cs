using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BlogTagConfiguration : IEntityTypeConfiguration<BlogTag>
{
    public void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.ToTable("BlogTags");

        builder.HasKey(bt => bt.Id);

        builder.HasIndex(bt => new { bt.BlogId, bt.TagId })
            .IsUnique();

        builder.HasOne(bt => bt.Blog)
            .WithMany()
            .HasForeignKey(bt => bt.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bt => bt.Tag)
            .WithMany()
            .HasForeignKey(bt => bt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
