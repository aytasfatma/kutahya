using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReferenceProjectImageConfiguration : IEntityTypeConfiguration<ReferenceProjectImage>
{
    public void Configure(EntityTypeBuilder<ReferenceProjectImage> builder)
    {
        builder.ToTable("ReferenceProjectImages");

        builder.HasKey(rpi => rpi.Id);

        builder.Property(rpi => rpi.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rpi => rpi.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rpi => rpi.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(rpi => rpi.ReferenceProject)
            .WithMany()
            .HasForeignKey(rpi => rpi.ReferenceProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bir projede en fazla bir IsFeatured=true kaydı olabilir — ProductImage'daki filtered unique
        // index deseninin (ADR-013) tekrarı.
        builder.HasIndex(rpi => rpi.ReferenceProjectId)
            .IsUnique()
            .HasFilter("[IsFeatured] = 1")
            .HasDatabaseName("IX_ReferenceProjectImages_ReferenceProjectId_IsFeatured");
    }
}
