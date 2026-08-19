using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductDocumentConfiguration : IEntityTypeConfiguration<ProductDocument>
{
    public void Configure(EntityTypeBuilder<ProductDocument> builder)
    {
        builder.ToTable("ProductDocuments");

        builder.HasKey(pd => pd.Id);

        builder.HasIndex(pd => new { pd.ProductId, pd.DocumentId })
            .IsUnique();

        builder.HasOne(pd => pd.Product)
            .WithMany()
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pd => pd.Document)
            .WithMany()
            .HasForeignKey(pd => pd.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
