using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DealerImageConfiguration : IEntityTypeConfiguration<DealerImage>
{
    public void Configure(EntityTypeBuilder<DealerImage> builder)
    {
        builder.ToTable("DealerImages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.IsFeatured).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);
        builder.HasOne(x => x.Dealer).WithMany().HasForeignKey(x => x.DealerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.DealerId).IsUnique().HasFilter("[IsFeatured] = 1")
            .HasDatabaseName("IX_DealerImages_DealerId_IsFeatured");
        builder.HasIndex(x => new { x.DealerId, x.DisplayOrder });
    }
}
