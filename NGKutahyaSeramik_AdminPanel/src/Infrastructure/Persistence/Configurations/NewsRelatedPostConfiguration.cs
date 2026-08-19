using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NewsRelatedPostConfiguration : IEntityTypeConfiguration<NewsRelatedPost>
{
    public void Configure(EntityTypeBuilder<NewsRelatedPost> builder)
    {
        builder.ToTable("NewsRelatedPosts");

        builder.HasKey(nrp => nrp.Id);

        builder.HasIndex(nrp => new { nrp.NewsId, nrp.RelatedNewsId })
            .IsUnique();

        builder.HasOne(nrp => nrp.News)
            .WithMany()
            .HasForeignKey(nrp => nrp.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing FK çifti aynı anda Cascade olamaz (SQL Server "multiple cascade paths"
        // hatası verir) — bu yüzden RelatedNewsId Restrict, temizliği NewsService.DeleteAsync
        // silme öncesi elle yapıyor (bkz. INewsRepository.RemoveRelatedPostReferencesAsync).
        builder.HasOne(nrp => nrp.RelatedNews)
            .WithMany()
            .HasForeignKey(nrp => nrp.RelatedNewsId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
