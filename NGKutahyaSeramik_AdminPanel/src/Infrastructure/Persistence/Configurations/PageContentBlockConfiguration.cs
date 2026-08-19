using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PageContentBlockConfiguration : IEntityTypeConfiguration<PageContentBlock>
{
    public void Configure(EntityTypeBuilder<PageContentBlock> builder)
    {
        builder.ToTable("PageContentBlocks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BlockType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(b => b.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.ImagePath)
            .HasMaxLength(500);

        builder.Property(b => b.VideoEmbedUrl)
            .HasMaxLength(500);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // Page 1 --- N PageContentBlock (bire-çok): bir Page'in birden fazla bloğu olabilir, her blok
        // tek bir Page'e aittir (tek-sahipli — ProductImage/ReferenceProjectImage ile aynı desen).
        // Page silindiğinde bloklar da silinir — bloğun Page'siz bağımsız bir varlığı yok
        // (Document'ın M2M/opsiyonel ilişkisinden farklı).
        builder.HasOne(b => b.Page)
            .WithMany()
            .HasForeignKey(b => b.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        // PageContentBlockRepository.GetByPageIdAsync her zaman "WHERE PageId=@p ORDER BY
        // DisplayOrder" çalıştırır (Task 12 index audit) — composite index, FK için EF'in otomatik
        // oluşturacağı tekil PageId index'inin yerini de alır (leftmost-prefix zaten PageId).
        builder.HasIndex(b => new { b.PageId, b.DisplayOrder })
            .HasDatabaseName("IX_PageContentBlocks_PageId_DisplayOrder");
    }
}
