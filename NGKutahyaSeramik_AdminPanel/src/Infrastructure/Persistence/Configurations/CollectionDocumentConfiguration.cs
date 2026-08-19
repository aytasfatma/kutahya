using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CollectionDocumentConfiguration : IEntityTypeConfiguration<CollectionDocument>
{
    public void Configure(EntityTypeBuilder<CollectionDocument> builder)
    {
        builder.ToTable("CollectionDocuments");

        builder.HasKey(cd => cd.Id);

        builder.HasIndex(cd => new { cd.CollectionId, cd.DocumentId })
            .IsUnique();

        builder.HasOne(cd => cd.Collection)
            .WithMany()
            .HasForeignKey(cd => cd.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cd => cd.Document)
            .WithMany()
            .HasForeignKey(cd => cd.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
