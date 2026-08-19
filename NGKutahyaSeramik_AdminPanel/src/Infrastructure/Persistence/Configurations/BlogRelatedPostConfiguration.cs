using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BlogRelatedPostConfiguration : IEntityTypeConfiguration<BlogRelatedPost>
{
    public void Configure(EntityTypeBuilder<BlogRelatedPost> builder)
    {
        builder.ToTable("BlogRelatedPosts");

        builder.HasKey(brp => brp.Id);

        builder.HasIndex(brp => new { brp.BlogId, brp.RelatedBlogId })
            .IsUnique();

        builder.HasOne(brp => brp.Blog)
            .WithMany()
            .HasForeignKey(brp => brp.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing FK çifti aynı anda Cascade olamaz (SQL Server "multiple cascade paths"
        // hatası verir) — bu yüzden RelatedBlogId Restrict, temizliği BlogService.DeleteAsync
        // silme öncesi elle yapıyor (bkz. IBlogRepository.RemoveRelatedPostReferencesAsync).
        builder.HasOne(brp => brp.RelatedBlog)
            .WithMany()
            .HasForeignKey(brp => brp.RelatedBlogId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
