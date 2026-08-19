using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ImageType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pi => pi.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pi => pi.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pi => pi.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(pi => pi.Product)
            .WithMany()
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bir üründe en fazla bir IsPrimary=true kaydı olabilir — DB seviyesinde filtered unique index.
        builder.HasIndex(pi => pi.ProductId)
            .IsUnique()
            .HasFilter("[IsPrimary] = 1")
            .HasDatabaseName("IX_ProductImages_ProductId_IsPrimary");

        // ProductImageRepository.GetByProductIdAsync her zaman "WHERE ProductId=@p ORDER BY
        // DisplayOrder" sorgusu çalıştırır (gerçek SQL ORDER BY — bkz. Task 12 index audit) —
        // yukarıdaki filtered unique index yalnızca IsPrimary=1 satırını kapsadığı için bu genel
        // sıralı listeleme sorgusuna hizmet etmez, ayrı bir composite index gerekli.
        builder.HasIndex(pi => new { pi.ProductId, pi.DisplayOrder })
            .HasDatabaseName("IX_ProductImages_ProductId_DisplayOrder");
    }
}
